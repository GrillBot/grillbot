using Discord;
using Discord.Net;
using Discord.WebSocket;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers;
using GrillBot.Core.Extensions;
using GrillBot.Core.Managers.Performance;
using GrillBot.Core.Redis.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

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
    private readonly ICounterManager _counterManager;
    private readonly IServiceProvider _serviceProvider;

    public MessageCacheManager(DiscordSocketClient discordClient, InitManager initManager, ICounterManager counterManager, IServiceProvider serviceProvider)
    {
        _initManager = initManager;
        _counterManager = counterManager;
        _serviceProvider = serviceProvider;

        discordClient.MessageReceived += OnMessageReceivedAsync;
        discordClient.MessageDeleted += OnMessageDeletedAsync;
        discordClient.ChannelDestroyed += OnChannelDeletedAsync;
        discordClient.ThreadDeleted += OnThreadDeletedAsync;
        discordClient.MessageUpdated += OnMessageUpdatedAsync;
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
            await ClearDeadRecordsInCacheAsync(message.Channel);

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

        var pattern = CreateCacheKeyPattern(channel);

        await _writerLock.WaitAsync();
        await _readerLock.WaitAsync();
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var server = scope.ServiceProvider.GetRequiredService<IServer>();

            await foreach (var key in server.KeysAsync(pattern: pattern, pageSize: int.MaxValue))
            {
                var messageId = key.ToString().Replace(pattern.Replace("*", ""), "").ToUlong();

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

        var pattern = CreateCacheKeyPattern(thread.Value);

        await _writerLock.WaitAsync();
        await _readerLock.WaitAsync();
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var server = scope.ServiceProvider.GetRequiredService<IServer>();

            await foreach (var key in server.KeysAsync(pattern: pattern, pageSize: int.MaxValue))
            {
                var messageId = key.ToString().Replace(pattern.Replace("*", ""), "").ToUlong();

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
                    ? [.. await channel.GetMessagesAsync(range.Value.messageId, range.Value.direction, limit).FlattenAsync()]
                    : [.. await channel.GetMessagesAsync(limit).FlattenAsync()];
            }
            catch (HttpException ex) when (IsApiExpectedError(ex))
            {
                // Catches expected errors from discord API.
                return [];
            }
            catch(ArgumentNullException)
            {
                // TODO Delete after Discord.NET release with https://github.com/discord-net/Discord.Net/pull/3065
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
        var cacheKeys = newMessages.ConvertAll(CreateCacheKey);

        using var scope = _serviceProvider.CreateScope();

        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
        foreach (var key in cacheKeys)
            await cache.SetAsync(key, 0, TimeSpan.FromDays(6 * 31));
    }

    private async Task RemoveIndexAsync(IMessage message)
    {
        var cacheKey = CreateCacheKey(message);

        using var scope = _serviceProvider.CreateScope();

        var database = scope.ServiceProvider.GetRequiredService<IDatabase>();
        await database.KeyDeleteAsync(cacheKey);
    }

    public async Task<int> GetCachedMessagesCount(IChannel channel)
    {
        var count = 0;
        var pattern = CreateCacheKeyPattern(channel);

        using var scope = _serviceProvider.CreateScope();
        var server = scope.ServiceProvider.GetRequiredService<IServer>();

        await _readerLock.WaitAsync();
        try
        {
            await foreach (var key in server.KeysAsync(pattern: pattern, pageSize: int.MaxValue))
            {
                var messageId = key.ToString().Replace(pattern.Replace("*", ""), "").ToUlong();

                if (_messages.ContainsKey(messageId))
                    count++;
            }

            return count;
        }
        finally
        {
            _readerLock.Release();
        }
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
        var pattern = CreateCacheKeyPattern(channel);

        await _writerLock.WaitAsync();
        await _readerLock.WaitAsync();
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var server = scope.ServiceProvider.GetRequiredService<IServer>();
            var count = 0;

            await foreach (var key in server.KeysAsync(pattern: pattern, pageSize: int.MaxValue))
            {
                var messageId = key.ToString().Replace(pattern.Replace("*", ""), "").ToUlong();

                _deletedMessages.Add(messageId);
                _messagesForUpdate.Remove(messageId);

                count++;
            }

            return count;
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
                if (!_messages.Remove(id, out var msg))
                    continue;

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

    private static string CreateCacheKey(IMessage message) => $"GrillBot_MessageIndex_{message.Channel.Id}_{message.Id}";
    private static string CreateCacheKeyPattern(IChannel channel) => $"GrillBot_MessageIndex_{channel.Id}_*";

    private async Task ClearDeadRecordsInCacheAsync(IChannel channel)
    {
        var pattern = CreateCacheKeyPattern(channel);

        using var scope = _serviceProvider.CreateScope();
        var server = scope.ServiceProvider.GetRequiredService<IServer>();
        var database = scope.ServiceProvider.GetRequiredService<IDatabase>();

        await foreach (var key in server.KeysAsync(pattern: pattern, pageSize: int.MaxValue))
        {
            var messageId = key.ToString().Replace(pattern.Replace("*", ""), "").ToUlong();

            if (!_messages.ContainsKey(messageId))
                await database.KeyDeleteAsync(key);
        }
    }
}
