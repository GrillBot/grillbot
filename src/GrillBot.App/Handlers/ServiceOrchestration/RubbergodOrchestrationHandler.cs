using GrillBot.App.Helpers;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using RubbergodService.Models.Events.Pins;

namespace GrillBot.App.Handlers.ServiceOrchestration;

public class RubbergodOrchestrationHandler : IMessageUpdatedEvent, IThreadDeletedEvent, IChannelDestroyedEvent
{
    private readonly IMessageCacheManager _messageCache;
    private readonly ChannelHelper _channelHelper;
    private readonly IRabbitPublisher _rabbitPublisher;

    public RubbergodOrchestrationHandler(IMessageCacheManager messageCache, ChannelHelper channelHelper, IRabbitPublisher rabbitPublisher)
    {
        _messageCache = messageCache;
        _channelHelper = channelHelper;
        _rabbitPublisher = rabbitPublisher;
    }

    // MessageUpdated
    public async Task ProcessAsync(Cacheable<IMessage, ulong> before, IMessage after, IMessageChannel channel)
    {
        if (!channel.IsPinSupported())
            return;

        var oldMessage = before.HasValue ? before.Value : null;
        oldMessage ??= await _messageCache.GetAsync(before.Id, null);
        if (oldMessage is null)
            return;

        var payloads = new List<ClearPinCachePayload>();
        var textChannel = (ITextChannel)channel;

        ProcessPinContentModified(payloads, oldMessage, after, textChannel);
        ProcessPinStateChange(payloads, oldMessage, after, textChannel);

        payloads = payloads.DistinctBy(o => $"{o.GuildId}/{o.ChannelId}").ToList();
        if (payloads.Count > 0)
            await _rabbitPublisher.PublishAsync(payloads);
    }

    // ThreadDeleted
    public async Task ProcessAsync(IThreadChannel? cachedThread, ulong threadId)
    {
        var guild = await _channelHelper.GetGuildFromChannelAsync(cachedThread, threadId);
        if (guild is not null)
            await _rabbitPublisher.PublishAsync(new ClearPinCachePayload(guild.Id.ToString(), threadId.ToString()));
    }

    // ChannelDestroyed
    public async Task ProcessAsync(IChannel channel)
    {
        if (!channel.IsPinSupported())
            return;

        var textChannel = (ITextChannel)channel;
        await _rabbitPublisher.PublishAsync(new ClearPinCachePayload(textChannel.GuildId.ToString(), textChannel.Id.ToString()));
    }

    private static void ProcessPinContentModified(List<ClearPinCachePayload> payloads, IMessage before, IMessage after, IGuildChannel guildChannel)
    {
        // Ignore non-pinned messages and non text channels.
        // Message not exists or nothing was changed.
        if (after.IsPinned && (before.Content != after.Content || before.Attachments.Count != after.Attachments.Count))
            payloads.Add(new ClearPinCachePayload(guildChannel.GuildId.ToString(), guildChannel.Id.ToString()));
    }

    private static void ProcessPinStateChange(List<ClearPinCachePayload> payloads, IMessage before, IMessage after, IGuildChannel guildChannel)
    {
        if (before.IsPinned != after.IsPinned)
            payloads.Add(new ClearPinCachePayload(guildChannel.GuildId.ToString(), guildChannel.Id.ToString()));
    }
}
