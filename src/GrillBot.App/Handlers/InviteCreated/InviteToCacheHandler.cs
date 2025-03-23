using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.InviteCreated;

public class InviteToCacheHandler(InviteManager _inviteManager) : IInviteCreatedEvent
{
    public Task ProcessAsync(IInviteMetadata invite)
        => _inviteManager.AddInviteAsync(invite);
}
