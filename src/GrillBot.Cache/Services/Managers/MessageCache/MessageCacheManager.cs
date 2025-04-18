using Discord;
using Discord.Net;
using Discord.WebSocket;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers;
using GrillBot.Core.Extensions;
using GrillBot.Core.Managers.Performance;
using Microsoft.Extensions.Caching.Distributed;

namespace GrillBot.Cache.Services.Managers.MessageCache;

public class MessageCacheManager : IMessageCacheManager
{
    private readonly SemaphoreSlim _readerLock = new(1);
    private readonly SemaphoreSlim _writerLock = new(1);

    private readonly Dictionary<ulong, IMessage> _messages = [];
    private readonly HashSet<ulong> _deletedMessages = [];
    private readonly HashSet<ulong> _loadedChannels = [];
    private readonly HashSet<ulong> _messagesForUpdate = [];

    private readonly InitManager _initManager;
    private readonly DiscordSocketClient _discordClient;
    private readonly GrillBotCacheBuilder _cacheBuilder;
    private readonly ICounterManager _counterManager;
    private readonly IDistributedCache _cache;

    public MessageCacheManager(DiscordSocketClient discordClient, InitManager initManager, GrillBotCacheBuilder cacheBuilder, ICounterManager counterManager,
        IDistributedCache cache)
    {
        _discordClient = discordClient;
        _initManager = initManager;
        _cacheBuilder = cacheBuilder;
        _counterManager = counterManager;
        _cache = cache;

        _discordClient.MessageReceived += OnMessageReceivedAsync;
        _discordClient.MessageDeleted += OnMessageDeletedAsync;
        _discordClient.ChannelDestroyed += OnChannelDeletedAsync;
        _discordClient.ThreadDeleted += OnThreadDeletedAsync;
        _discordClient.MessageUpdated += OnMessageUpdatedAsync;
    }

    private async Task OnMessageReceivedAsync(SocketMessage message)
    {
        if (message.Channel is IDMChannel || !_initManager.Get()) return;

        await _readerLock.WaitAsync();
        try
        {
            if (_loadedChannels.Contains(message.Channel.Id))
                return;
        }
        finally
        {
            _readerLock.Release();
        }

        await _writerLock.WaitAsync();
        await _readerLock.WaitAsync();
        try
        {
            await DownloadMessagesAsync(message.Channel, DiscordConfig.MaxMessagesPerBatch);
            _loadedChannels.Add(message.Channel.Id);
        }
        finally
        {
            _readerLock.Release();
            _writerLock.Release();
        }
    }

    private async Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> msg, Cacheable<IMessageChannel, ulong> channel)
    {
        if (!channel.HasValue || channel.Value is IDMChannel || !_initManager.Get()) return;

        await _writerLock.WaitAsync();
        await _readerLock.WaitAsync();
        try
        {
            _deletedMessages.Add(msg.Id);
            await DownloadMessagesAsync(channel.Value, msg.Id, Direction.Around, DiscordConfig.MaxMessagesPerBatch);
        }
        finally
        {
            _readerLock.Release();
            _writerLock.Release();
        }
    }

    private async Task OnChannelDeletedAsync(SocketChannel channel)
    {
        if (channel is IDMChannel || !_initManager.Get()) return;

        await _writerLock.WaitAsync();
        await _readerLock.WaitAsync();
        try
        {
            using var cache = _cacheBuilder.CreateRepository();

            var messages = await cache.MessageIndexRepository.GetMessagesAsync(channelId: channel.Id);
            foreach (var messageId in messages.Select(o => o.MessageId.ToUlong()))
            {
                _deletedMessages.Add(messageId);
                _messagesForUpdate.Remove(messageId);
            }
        }
        finally
        {
            _readerLock.Release();
            _writerLock.Release();
        }
    }

    private async Task OnThreadDeletedAsync(Cacheable<SocketThreadChannel, ulong> thread)
    {
        if (!thread.HasValue || !_initManager.Get()) return;

        await _writerLock.WaitAsync();
        await _readerLock.WaitAsync();
        try
        {
            using var cache = _cacheBuilder.CreateRepository();

            var messages = await cache.MessageIndexRepository.GetMessagesAsync(channelId: thread.Value.Id, guildId: thread.Value.Guild.Id);
            foreach (var messageId in messages.Select(o => o.MessageId.ToUlong()))
            {
                _deletedMessages.Add(messageId);
                _messagesForUpdate.Remove(messageId);
            }
        }
        finally
        {
            _readerLock.Release();
            _writerLock.Release();
        }
    }

    private async Task OnMessageUpdatedAsync(Cacheable<IMessage, ulong> _, SocketMessage after, ISocketMessageChannel channel)
    {
        if (channel is IDMChannel || !_initManager.Get()) return;

        await _writerLock.WaitAsync();
        await _readerLock.WaitAsync();
        try
        {
            _messagesForUpdate.Add(after.Id);
        }
        finally
        {
            _readerLock.Release();
            _writerLock.Release();
        }
    }

    private async Task DownloadMessagesAsync(IMessageChannel channel, ulong messageId, Direction direction, int limit)
    {
        var messages = await DownloadMessagesFromChannelAsync(channel, (messageId, direction), limit);
        await ProcessDownloadedMessages(messages, false);
    }

    private async Task DownloadMessagesAsync(IMessageChannel channel, int limit)
    {
        var messages = await DownloadMessagesFromChannelAsync(channel, null, limit);
        await ProcessDownloadedMessages(messages, false);
    }

    private async Task<IMessage?> DownloadMessageFromChannelAsync(IMessageChannel channel, ulong id)
    {
        using (_counterManager.Create("Discord.API.Messages"))
        {
            try
            {
                return await channel.GetMessageAsync(id);
            }
            catch (HttpException ex) when (IsApiExpectedError(ex))
            {
                // Catches expected errors from discord API.
                return null;
            }
        }
    }

    private async Task<List<IMessage>> DownloadMessagesFromChannelAsync(IMessageChannel channel, (ulong messageId, Direction direction)? range, int limit)
    {
        using (_counterManager.Create("Discord.API.Messages"))
        {
            try
            {
                return range is not null
                    ? [.. (await channel.GetMessagesAsync(range.Value.messageId, range.Value.direction, limit).FlattenAsync())]
                    : [.. (await channel.GetMessagesAsync(limit).FlattenAsync())];
            }
            catch (HttpException ex) when (IsApiExpectedError(ex))
            {
                // Catches expected errors from discord API.
                return [];
            }
        }
    }

    /// <summary>
    /// Checks if error created from discord api is expected.
    /// Expected is InternalServerError, ServiceUnavailable, UnknownChannel and UnknownMessage.
    /// </summary>
    private static bool IsApiExpectedError(HttpException ex)
        => ex.IsExpectedOutageError() || ex.DiscordCode == DiscordErrorCode.UnknownChannel || ex.DiscordCode == DiscordErrorCode.UnknownMessage;

    private async Task ProcessDownloadedMessages(List<IMessage> messages, bool forceDelete)
    {
        if (forceDelete)
        {
            foreach (var msg in messages)
            {
                await RemoveIndexAsync(msg);
                _messages.Remove(msg.Id);
            }
        }

        var newMessages = messages.FindAll(o => !_messages.ContainsKey(o.Id));
        if (newMessages.Count > 0)
        {
            newMessages.ForEach(o => _messages.Add(o.Id, o));
            await CreateIndexesAsync(newMessages);
        }
    }

    private async Task CreateIndexesAsync(List<IMessage> newMessages)
    {
        var entities = newMessages.ConvertAll(o => new Entity.MessageIndex
        {
            MessageId = o.Id.ToString(),
            AuthorId = o.Author.Id.ToString(),
            ChannelId = o.Channel.Id.ToString(),
            GuildId = ((IGuildChannel)o.Channel).GuildId.ToString()
        });

        using var cache = _cacheBuilder.CreateRepository();

        await cache.AddCollectionAsync(entities);
        await cache.CommitAsync();
    }

    private async Task RemoveIndexAsync(IMessage message)
    {
        using var cache = _cacheBuilder.CreateRepository();

        var msgIndex = await cache.MessageIndexRepository.FindMessageByIdAsync(message.Id);
        if (msgIndex is not null)
        {
            cache.Remove(msgIndex);
            await cache.CommitAsync();
        }
    }

    public async Task<int> GetCachedMessagesCount(IChannel channel)
    {
        using var cache = _cacheBuilder.CreateRepository();

        return await cache.MessageIndexRepository.GetMessagesCountAsync(channelId: channel.Id);
    }

    public async Task<IMessage?> GetAsync(ulong messageId, IMessageChannel? channel, bool includeRemoved = false, bool forceReload = false)
    {
        await _readerLock.WaitAsync();
        try
        {
            if (!includeRemoved && _deletedMessages.Contains(messageId))
                return null;
            if (_messages.TryGetValue(messageId, out var value) && !forceReload)
                return value;
        }
        finally
        {
            _readerLock.Release();
        }

        if (channel is null) return null;

        await _writerLock.WaitAsync();
        await _readerLock.WaitAsync();
        try
        {
            var message = await DownloadMessageFromChannelAsync(channel, messageId);
            if (message is null) return null;

            await ProcessDownloadedMessages([message], forceReload);
            return message;
        }
        finally
        {
            _readerLock.Release();
            _writerLock.Release();
        }
    }

    public async Task<int> ClearAllMessagesFromChannelAsync(IChannel channel)
    {
        await _writerLock.WaitAsync();
        await _readerLock.WaitAsync();
        try
        {
            using var cache = _cacheBuilder.CreateRepository();
            var messages = (await cache.MessageIndexRepository.GetMessagesAsync(channelId: channel.Id))
                .ConvertAll(o => o.MessageId.ToUlong());

            foreach (var msgId in messages)
            {
                _deletedMessages.Add(msgId);
                _messagesForUpdate.Remove(msgId);
            }

            return messages.Count;
        }
        finally
        {
            _readerLock.Release();
            _writerLock.Release();
        }
    }

    public async Task DeleteAsync(ulong messageId)
    {
        await _writerLock.WaitAsync();
        await _readerLock.WaitAsync();
        try
        {
            _deletedMessages.Add(messageId);
            _messagesForUpdate.Remove(messageId);
        }
        finally
        {
            _readerLock.Release();
            _writerLock.Release();
        }
    }

    public async Task<string> ProcessScheduledTaskAsync()
    {
        var report = new List<string>();

        await ProcessDeletedMessagesAsync(report);
        await ProcessUpdatedMessagesAsync(report);
        return string.Join("\n", report);
    }

    private async Task ProcessDeletedMessagesAsync(List<string> report)
    {
        await _writerLock.WaitAsync();
        await _readerLock.WaitAsync();
        try
        {
            foreach (var id in _deletedMessages)
            {
                if (!_messages.Remove(id, out var msg)) continue;

                await RemoveIndexAsync(msg);
                report.Add($"Removed {id} (Author: {msg.Author.GetFullName()}, Channel: {msg.Channel.Name}, CreatedAt: {msg.CreatedAt.LocalDateTime})");
            }

            _deletedMessages.Clear();
        }
        finally
        {
            _readerLock.Release();
            _writerLock.Release();
        }
    }

    private async Task ProcessUpdatedMessagesAsync(List<string> report)
    {
        await _writerLock.WaitAsync();
        await _readerLock.WaitAsync();
        try
        {
            foreach (var id in _messagesForUpdate)
            {
                if (!_messages.Remove(id, out var msg)) continue;

                var message = await DownloadMessageFromChannelAsync(msg.Channel, id);
                if (message is null)
                {
                    _deletedMessages.Add(id);
                    continue;
                }

                _messages.Add(id, message);
                report.Add($"Refreshed {id} (Author: {msg.Author.GetFullName()}, Channel: {msg.Channel.Name}, CreatedAt: {msg.CreatedAt.LocalDateTime})");
            }

            _messagesForUpdate.Clear();
        }
        finally
        {
            _readerLock.Release();
            _writerLock.Release();
        }
    }
}
