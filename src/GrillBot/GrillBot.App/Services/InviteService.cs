using GrillBot.App.Infrastructure;
using GrillBot.Cache.Services.Managers;

namespace GrillBot.App.Services;

[Initializable]
public class InviteService
{
    private DiscordSocketClient DiscordClient { get; }
    private InviteManager InviteManager { get; }

    public InviteService(DiscordSocketClient discordClient, InviteManager inviteManager)
    {
        DiscordClient = discordClient;
        InviteManager = inviteManager;

        DiscordClient.InviteCreated += OnInviteCreated;
    }

    private async Task OnInviteCreated(IInviteMetadata invite)
        => await InviteManager.AddInviteAsync(invite);
}
