using System.Net;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers;
using GrillBot.Core.Extensions;
using GrillBot.Core.Managers.Performance;

namespace GrillBot.Cache.Services.Managers.MessageCache;

public class MessageCacheManager : IMessageCacheManager
{
    private SemaphoreSlim ReaderLock { get; }
    private SemaphoreSlim WriterLock { get; }

    private Dictionary<ulong, IMessage> Messages { get; }
    private HashSet<ulong> DeletedMessages { get; }
    private HashSet<ulong> LoadedChannels { get; }
    private HashSet<ulong> MessagesForUpdate { get; }

    private InitManager InitManager { get; }
    private DiscordSocketClient DiscordClient { get; }
    private GrillBotCacheBuilder CacheBuilder { get; }
    private ICounterManager CounterManager { get; }

    public MessageCacheManager(DiscordSocketClient discordClient, InitManager initManager, GrillBotCacheBuilder cacheBuilder, ICounterManager counterManager)
    {
        DiscordClient = discordClient;
        InitManager = initManager;
        CacheBuilder = cacheBuilder;
        CounterManager = counterManager;

        ReaderLock = new SemaphoreSlim(1);
        WriterLock = new SemaphoreSlim(1);
        Messages = new Dictionary<ulong, IMessage>();
        DeletedMessages = new HashSet<ulong>();
        LoadedChannels = new HashSet<ulong>();
        MessagesForUpdate = new HashSet<ulong>();

        DiscordClient.MessageReceived += OnMessageReceivedAsync;
        DiscordClient.MessageDeleted += OnMessageDeletedAsync;
        DiscordClient.ChannelDestroyed += OnChannelDeletedAsync;
        DiscordClient.ThreadDeleted += OnThreadDeletedAsync;
        DiscordClient.MessageUpdated += OnMessageUpdatedAsync;
    }

    private async Task OnMessageReceivedAsync(SocketMessage message)
    {
        if (message.Channel is IDMChannel || !InitManager.Get()) return;

        await ReaderLock.WaitAsync();
        try
        {
            if (LoadedChannels.Contains(message.Channel.Id))
                return;
        }
        finally
        {
            ReaderLock.Release();
        }

        await WriterLock.WaitAsync();
        await ReaderLock.WaitAsync();
        try
        {
            await DownloadMessagesAsync(message.Channel, DiscordConfig.MaxMessagesPerBatch);
            LoadedChannels.Add(message.Channel.Id);
        }
        finally
        {
            ReaderLock.Release();
            WriterLock.Release();
        }
    }

    private async Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> msg, Cacheable<IMessageChannel, ulong> channel)
    {
        if (!channel.HasValue || channel.Value is IDMChannel || !InitManager.Get()) return;

        await WriterLock.WaitAsync();
        await ReaderLock.WaitAsync();
        try
        {
            DeletedMessages.Add(msg.Id);
            await DownloadMessagesAsync(channel.Value, msg.Id, Direction.Around, DiscordConfig.MaxMessagesPerBatch);
        }
        finally
        {
            ReaderLock.Release();
            WriterLock.Release();
        }
    }

    private async Task OnChannelDeletedAsync(SocketChannel channel)
    {
        if (channel is IDMChannel || !InitManager.Get()) return;

        await WriterLock.WaitAsync();
        await ReaderLock.WaitAsync();
        try
        {
            await using var cache = CacheBuilder.CreateRepository();

            var messages = await cache.MessageIndexRepository.GetMessagesAsync(channelId: channel.Id);
            foreach (var messageId in messages.Select(o => o.MessageId.ToUlong()))
            {
                DeletedMessages.Add(messageId);
                if (MessagesForUpdate.Contains(messageId))
                    MessagesForUpdate.Remove(messageId);
            }
        }
        finally
        {
            ReaderLock.Release();
            WriterLock.Release();
        }
    }

    private async Task OnThreadDeletedAsync(Cacheable<SocketThreadChannel, ulong> thread)
    {
        if (!thread.HasValue || !InitManager.Get()) return;

        await WriterLock.WaitAsync();
        await ReaderLock.WaitAsync();
        try
        {
            await using var cache = CacheBuilder.CreateRepository();

            var messages = await cache.MessageIndexRepository.GetMessagesAsync(channelId: thread.Value.Id, guildId: thread.Value.Guild.Id);
            foreach (var messageId in messages.Select(o => o.MessageId.ToUlong()))
            {
                DeletedMessages.Add(messageId);
                if (MessagesForUpdate.Contains(messageId))
                    MessagesForUpdate.Remove(messageId);
            }
        }
        finally
        {
            ReaderLock.Release();
            WriterLock.Release();
        }
    }

    private async Task OnMessageUpdatedAsync(Cacheable<IMessage, ulong> _, SocketMessage after, ISocketMessageChannel channel)
    {
        if (channel is IDMChannel || !InitManager.Get()) return;

        await WriterLock.WaitAsync();
        await ReaderLock.WaitAsync();
        try
        {
            MessagesForUpdate.Add(after.Id);
        }
        finally
        {
            ReaderLock.Release();
            WriterLock.Release();
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
        using (CounterManager.Create("Discord.API.Messages"))
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
        using (CounterManager.Create("Discord.API.Messages"))
        {
            try
            {
                return range is not null
                    ? (await channel.GetMessagesAsync(range.Value.messageId, range.Value.direction, limit).FlattenAsync()).ToList()
                    : (await channel.GetMessagesAsync(limit).FlattenAsync()).ToList();
            }
            catch (HttpException ex) when (IsApiExpectedError(ex))
            {
                // Catches expected errors from discord API.
                return new List<IMessage>();
            }
        }
    }

    /// <summary>
    /// Checks if error created from discord api is expected.
    /// Expected is InternalServerError, ServiceUnavailable, UnknownChannel and UnknownMessage. 
    /// </summary>
    private static bool IsApiExpectedError(HttpException ex)
    {
        return ex.HttpCode == HttpStatusCode.InternalServerError || ex.HttpCode == HttpStatusCode.ServiceUnavailable || ex.DiscordCode == DiscordErrorCode.UnknownChannel ||
               ex.DiscordCode == DiscordErrorCode.UnknownMessage;
    }

    private async Task ProcessDownloadedMessages(List<IMessage> messages, bool forceDelete)
    {
        if (forceDelete)
        {
            foreach (var msg in messages)
            {
                await RemoveIndexAsync(msg);
                Messages.Remove(msg.Id);
            }
        }

        var newMessages = messages.FindAll(o => !Messages.ContainsKey(o.Id));
        if (newMessages.Count > 0)
        {
            newMessages.ForEach(o => Messages.Add(o.Id, o));
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

        await using var cache = CacheBuilder.CreateRepository();

        await cache.AddRangeAsync(entities);
        await cache.CommitAsync();
    }

    private async Task RemoveIndexAsync(IMessage message)
    {
        await using var cache = CacheBuilder.CreateRepository();

        var msgIndex = await cache.MessageIndexRepository.FindMessageByIdAsync(message.Id);
        if (msgIndex is not null)
        {
            cache.Remove(msgIndex);
            await cache.CommitAsync();
        }
    }

    public async Task<int> GetCachedMessagesCount(IChannel channel)
    {
        await using var cache = CacheBuilder.CreateRepository();

        return await cache.MessageIndexRepository.GetMessagesCountAsync(channelId: channel.Id);
    }

    public async Task<IMessage?> GetAsync(ulong messageId, IMessageChannel? channel, bool includeRemoved = false, bool forceReload = false)
    {
        await ReaderLock.WaitAsync();
        try
        {
            if (!includeRemoved && DeletedMessages.Contains(messageId))
                return null;
            if (Messages.TryGetValue(messageId, out var value) && !forceReload)
                return value;
        }
        finally
        {
            ReaderLock.Release();
        }

        if (channel is null) return null;

        await WriterLock.WaitAsync();
        await ReaderLock.WaitAsync();
        try
        {
            var message = await DownloadMessageFromChannelAsync(channel, messageId);
            if (message is null) return null;

            await ProcessDownloadedMessages(new List<IMessage> { message }, forceReload);
            return message;
        }
        finally
        {
            ReaderLock.Release();
            WriterLock.Release();
        }
    }

    public async Task<int> ClearAllMessagesFromChannelAsync(IChannel channel)
    {
        await WriterLock.WaitAsync();
        await ReaderLock.WaitAsync();
        try
        {
            await using var cache = CacheBuilder.CreateRepository();
            var messages = (await cache.MessageIndexRepository.GetMessagesAsync(channelId: channel.Id))
                .ConvertAll(o => o.MessageId.ToUlong());

            foreach (var msgId in messages)
            {
                DeletedMessages.Add(msgId);

                if (MessagesForUpdate.Contains(msgId))
                    MessagesForUpdate.Remove(msgId);
            }

            return messages.Count;
        }
        finally
        {
            ReaderLock.Release();
            WriterLock.Release();
        }
    }

    public async Task<string> ProcessScheduledTaskAsync()
    {
        var report = new List<string>();

        await ProcessDeletedMessagesAsync(report);
        await ProcessUpdatedMessagesAsync(report);
        return string.Join("\n", report);
    }

    private async Task ProcessDeletedMessagesAsync(ICollection<string> report)
    {
        await WriterLock.WaitAsync();
        await ReaderLock.WaitAsync();
        try
        {
            foreach (var id in DeletedMessages)
            {
                if (!Messages.Remove(id, out var msg)) continue;

                await RemoveIndexAsync(msg);
                report.Add($"Removed {id} (Author: {msg.Author.GetFullName()}, Channel: {msg.Channel.Name}, CreatedAt: {msg.CreatedAt.LocalDateTime})");
            }

            DeletedMessages.Clear();
        }
        finally
        {
            ReaderLock.Release();
            WriterLock.Release();
        }
    }

    private async Task ProcessUpdatedMessagesAsync(ICollection<string> report)
    {
        await WriterLock.WaitAsync();
        await ReaderLock.WaitAsync();
        try
        {
            foreach (var id in MessagesForUpdate)
            {
                if (!Messages.Remove(id, out var msg)) continue;

                var message = await DownloadMessageFromChannelAsync(msg.Channel, id);
                if (message is null)
                {
                    DeletedMessages.Add(id);
                    continue;
                }

                Messages.Add(id, message);
                report.Add($"Refreshed {id} (Author: {msg.Author.GetFullName()}, Channel: {msg.Channel.Name}, CreatedAt: {msg.CreatedAt.LocalDateTime})");
            }

            MessagesForUpdate.Clear();
        }
        finally
        {
            ReaderLock.Release();
            WriterLock.Release();
        }
    }
}
