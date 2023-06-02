using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Services.RubbergodService;

namespace GrillBot.App.Handlers.Synchronization.Services;

public class RubbergodServiceSynchronizationHandler : BaseSynchronizationHandler<IRubbergodServiceClient>, IUserUpdatedEvent, IMessageReceivedEvent
{
    public RubbergodServiceSynchronizationHandler(IRubbergodServiceClient serviceClient, GrillBotDatabaseBuilder databaseBuilder) : base(serviceClient, databaseBuilder)
    {
    }

    // UserUpdated
    public async Task ProcessAsync(IUser before, IUser after)
    {
        if (before.Username == after.Username && before.Discriminator == after.Discriminator && before.GetUserAvatarUrl() == after.GetUserAvatarUrl())
            return;

        await ServiceClient.RefreshMemberAsync(after.Id);
    }

    // MessageReceived
    public async Task ProcessAsync(IMessage message)
    {
        await ProcessPinChangesAsync(message);
    }

    private async Task ProcessPinChangesAsync(IMessage message)
    {
        if (message.Source != MessageSource.System || message.Type != MessageType.ChannelPinnedMessage || message.Channel is IVoiceChannel || message.Channel is not ITextChannel channel)
            return;

        await ServiceClient.InvalidatePinCacheAsync(channel.GuildId, channel.Id);
    }
}
