﻿using Discord;
using Discord.Net;
using Discord.WebSocket;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers;
using System.Net;

namespace GrillBot.Cache.Services.Managers;

public class MessageCacheManager
{
    private SemaphoreSlim Semaphore { get; }
    private Dictionary<ulong, IMessage> Messages { get; }
    private HashSet<ulong> DeletedMessages { get; }
    private HashSet<ulong> LoadedChannels { get; }
    private HashSet<ulong> MessagesForUpdate { get; }

    private InitManager InitManager { get; }
    private DiscordSocketClient DiscordClient { get; }
    private GrillBotCacheBuilder CacheBuilder { get; }

    public MessageCacheManager(DiscordSocketClient discordClient, InitManager initManager, GrillBotCacheBuilder cacheBuilder)
    {
        DiscordClient = discordClient;
        InitManager = initManager;
        CacheBuilder = cacheBuilder;

        Semaphore = new(1);
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
        await Semaphore.WaitAsync();

        try
        {
            if (!InitManager.Get() || LoadedChannels.Contains(message.Channel.Id)) return;

            await DownloadMessagesAsync(message.Channel); // Download 100 latest messages.
        }
        finally
        {
            Semaphore.Release();
        }
    }

    private async Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> msg, Cacheable<IMessageChannel, ulong> channel)
    {
        await Semaphore.WaitAsync();

        try
        {
            DeletedMessages.Add(msg.Id);

            if (channel.HasValue && channel.Value is not IDMChannel)
                await DownloadMessagesAsync(channel.Value, msg.Id, Direction.Around); // Download 100 messages around deleted message.
        }
        finally
        {
            Semaphore.Release();
        }
    }

    private async Task OnChannelDeletedAsync(SocketChannel channel)
    {
        await Semaphore.WaitAsync();

        try
        {
            using var cache = CacheBuilder.CreateRepository();

            var messages = await cache.MessageIndexRepository.GetMessagesAsync(channelId: channel.Id);
            foreach (var msg in messages)
                DeletedMessages.Add(msg.MessageId.ToUlong());
        }
        finally
        {
            Semaphore.Release();
        }
    }

    private async Task OnThreadDeletedAsync(Cacheable<SocketThreadChannel, ulong> thread)
    {
        if (!thread.HasValue) return;
        await Semaphore.WaitAsync();

        try
        {
            using var cache = CacheBuilder.CreateRepository();

            var messages = await cache.MessageIndexRepository.GetMessagesAsync(channelId: thread.Value.Id, guildId: thread.Value.Guild.Id);
            foreach (var msg in messages)
                DeletedMessages.Add(msg.MessageId.ToUlong());
        }
        finally
        {
            Semaphore.Release();
        }
    }

    private async Task OnMessageUpdatedAsync(Cacheable<IMessage, ulong> _, SocketMessage after, ISocketMessageChannel __)
    {
        await Semaphore.WaitAsync();

        try
        {
            MessagesForUpdate.Add(after.Id);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task DownloadMessagesAsync(IMessageChannel channel, ulong messageId, Direction direction, int limit = DiscordConfig.MaxMessagesPerBatch)
    {
        try
        {
            var messages = (await channel.GetMessagesAsync(messageId, direction, limit).FlattenAsync()).ToList();
            await ProcessDownloadedMessages(messages);
        }
        catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.InternalServerError)
        {
            // Catches errors from discord API. Internal server error are expected.
        }
    }

    public async Task DownloadMessagesAsync(IMessageChannel channel, int limit = DiscordConfig.MaxMessagesPerBatch)
    {
        try
        {
            var messages = (await channel.GetMessagesAsync(limit).FlattenAsync()).ToList();
            await ProcessDownloadedMessages(messages);
        }
        catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.InternalServerError)
        {
            // Catches errors from discord API. Internal server error are expected.
        }
    }

    private async Task ProcessDownloadedMessages(List<IMessage> messages)
    {
        var newMessages = messages.FindAll(o => !Messages.ContainsKey(o.Id));

        if (newMessages.Count > 0)
        {
            foreach (var msg in newMessages)
                Messages.Add(msg.Id, msg);

            await CreateIndexesAsync(newMessages);
        }
    }

    private async Task CreateIndexesAsync(List<IMessage> newMessages)
    {
        var entities = newMessages.ConvertAll(o => new Entity.MessageIndex()
        {
            MessageId = o.Id.ToString(),
            AuthorId = o.Author.Id.ToString(),
            ChannelId = o.Channel.Id.ToString(),
            GuildId = o.Channel is IGuildChannel guildChanel ? guildChanel.GuildId.ToString() : "0"
        });

        using var cache = CacheBuilder.CreateRepository();

        await cache.AddRangeAsync(entities);
        await cache.CommitAsync();
    }

    private async Task RemoveIndexAsync(IMessage message)
    {
        using var cache = CacheBuilder.CreateRepository();

        var msgIndex = await cache.MessageIndexRepository.FindMessageByIdAsync(message.Id);
        if (msgIndex != null)
        {
            cache.Remove(msgIndex);
            await cache.CommitAsync();
        }
    }

    public async Task<IMessage?> GetAsync(ulong messageId, IMessageChannel channel, bool includeRemoved = false)
    {
        await Semaphore.WaitAsync();

        try
        {
            if (!includeRemoved && DeletedMessages.Contains(messageId))
                return null;

            if (Messages.ContainsKey(messageId))
                return Messages[messageId];

            if (channel != null)
            {
                var message = await channel.GetMessageAsync(messageId);
                if (message == null)
                    return null;

                await ProcessDownloadedMessages(new() { message });
                return message;
            }

            return null;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task<int> ClearAllMessagesFromChannel(IChannel channel)
    {
        await Semaphore.WaitAsync();

        try
        {
            using var cache = CacheBuilder.CreateRepository();
            var messages = await cache.MessageIndexRepository.GetMessagesAsync(channelId: channel.Id);

            foreach (var msgId in messages.Select(o => o.MessageId.ToUlong()))
            {
                DeletedMessages.Add(msgId);

                if (MessagesForUpdate.Contains(msgId))
                    MessagesForUpdate.Remove(msgId);
            }

            return messages.Count;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task<string> ProcessScheduledTaskAsync()
    {
        await Semaphore.WaitAsync();

        var report = new List<string>();

        try
        {
            foreach (var id in DeletedMessages)
            {
                if (Messages.Remove(id, out var msg))
                {
                    await RemoveIndexAsync(msg);
                    report.Add($"Removed {id} (Author: {msg.Author.GetFullName()}, Channel: {msg.Channel.Name}, CreatedAt: {msg.CreatedAt.LocalDateTime})");
                }
            }
            DeletedMessages.Clear();

            foreach (var id in MessagesForUpdate)
            {
                if (Messages.Remove(id, out var msg))
                {
                    var message = await msg.Channel.GetMessageAsync(id);
                    if (message == null)
                    {
                        DeletedMessages.Add(id);
                        continue;
                    }

                    Messages.Add(id, message);
                    report.Add($"Refreshed {id} (Author: {msg.Author.GetFullName()}, Channel: {msg.Channel.Name}, CreatedAt: {msg.CreatedAt.LocalDateTime})");
                }
            }
            MessagesForUpdate.Clear();

            return string.Join("\n", report);
        }
        finally
        {
            Semaphore.Release();
        }
    }
}