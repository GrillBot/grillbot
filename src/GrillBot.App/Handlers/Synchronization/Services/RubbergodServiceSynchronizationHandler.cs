using GrillBot.App.Helpers;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Services.RubbergodService;

namespace GrillBot.App.Handlers.Synchronization.Services;

public class RubbergodServiceSynchronizationHandler : BaseSynchronizationHandler<IRubbergodServiceClient>, IUserUpdatedEvent, IMessageUpdatedEvent, IThreadDeletedEvent, IChannelDestroyedEvent
{
    private IMessageCacheManager MessageCache { get; }
    private ChannelHelper ChannelHelper { get; }

    public RubbergodServiceSynchronizationHandler(IRubbergodServiceClient serviceClient, GrillBotDatabaseBuilder databaseBuilder, IMessageCacheManager messageCache, ChannelHelper channelHelper) :
        base(serviceClient, databaseBuilder)
    {
        MessageCache = messageCache;
        ChannelHelper = channelHelper;
    }

    // UserUpdated
    public async Task ProcessAsync(IUser before, IUser after)
    {
        if (before.Username == after.Username && before.Discriminator == after.Discriminator && before.GetUserAvatarUrl() == after.GetUserAvatarUrl())
            return;

        await ServiceClient.RefreshMemberAsync(after.Id);
    }

    // MessageUpdated
    public async Task ProcessAsync(Cacheable<IMessage, ulong> before, IMessage after, IMessageChannel channel)
    {
        await ProcessPinContentModifiedAsync(before, after, channel);
        await ProcessPinStateChangeAsync(before, after, channel);
    }

    private async Task ProcessPinContentModifiedAsync(Cacheable<IMessage, ulong> before, IMessage after, IMessageChannel channel)
    {
        if (!after.IsPinned || channel is IVoiceChannel || channel is not ITextChannel textChannel) // Ignore non-pinned messages and non text channels.
            return;

        var oldMessage = before.HasValue ? before.Value : null;
        oldMessage ??= await MessageCache.GetAsync(before.Id, null);
        if (oldMessage is null || (oldMessage.Content == after.Content && oldMessage.Attachments.Count == after.Attachments.Count)) // Message not exists or nothing was changed.
            return;

        await ServiceClient.InvalidatePinCacheAsync(textChannel.GuildId, textChannel.Id);
    }

    private async Task ProcessPinStateChangeAsync(Cacheable<IMessage, ulong> before, IMessage after, IMessageChannel channel)
    {
        if (channel is IVoiceChannel || channel is not ITextChannel textChannel)
            return;

        var oldMessage = before.HasValue ? before.Value : null;
        oldMessage ??= await MessageCache.GetAsync(before.Id, null);
        if (oldMessage is null || oldMessage.IsPinned == after.IsPinned)
            return;

        await ServiceClient.InvalidatePinCacheAsync(textChannel.GuildId, textChannel.Id);
    }

    // ThreadDeleted
    public async Task ProcessAsync(IThreadChannel? cachedThread, ulong threadId)
    {
        var guild = await ChannelHelper.GetGuildFromChannelAsync(cachedThread, threadId);
        if (guild is null)
            return;
        
        await ServiceClient.InvalidatePinCacheAsync(guild.Id, threadId);
    }

    // ChannelDestroyed
    public async Task ProcessAsync(IChannel channel)
    {
        if (channel is IVoiceChannel || channel is not ITextChannel textChannel)
            return;

        await ServiceClient.InvalidatePinCacheAsync(textChannel.GuildId, textChannel.Id);
    }
}
