using GrillBot.App.Actions.Api.V1.Invite;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Models;

namespace GrillBot.App.Handlers.Ready;

public class InviteReadyHandler : IReadyEvent
{
    private ApiRequestContext ApiRequestContext { get; }
    private IDiscordClient DiscordClient { get; }
    private RefreshMetadata RefreshMetadata { get; }

    public InviteReadyHandler(ApiRequestContext context, IDiscordClient discordClient, RefreshMetadata refreshMetadata)
    {
        ApiRequestContext = context;
        DiscordClient = discordClient;
        RefreshMetadata = refreshMetadata;

        ApiRequestContext.LoggedUser = DiscordClient.CurrentUser;
    }

    public async Task ProcessAsync()
    {
        await RefreshMetadata.ProcessAsync(false);
    }
}
