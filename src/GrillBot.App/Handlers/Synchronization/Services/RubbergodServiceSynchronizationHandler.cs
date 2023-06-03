using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Services.RubbergodService;

namespace GrillBot.App.Handlers.Synchronization.Services;

public class RubbergodServiceSynchronizationHandler : BaseSynchronizationHandler<IRubbergodServiceClient>, IUserUpdatedEvent, IMessageUpdatedEvent
{
    private IMessageCacheManager MessageCache { get; }

    public RubbergodServiceSynchronizationHandler(IRubbergodServiceClient serviceClient, GrillBotDatabaseBuilder databaseBuilder, IMessageCacheManager messageCache) : base(serviceClient,
        databaseBuilder)
    {
        MessageCache = messageCache;
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
}
