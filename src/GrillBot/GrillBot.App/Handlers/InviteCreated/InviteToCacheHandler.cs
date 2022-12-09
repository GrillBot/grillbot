using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.InviteCreated;

public class InviteToCacheHandler : IInviteCreatedEvent
{
    private InviteManager InviteManager { get; }

    public InviteToCacheHandler(InviteManager inviteManager)
    {
        InviteManager = inviteManager;
    }

    public Task ProcessAsync(IInviteMetadata invite)
        => InviteManager.AddInviteAsync(invite);
}
