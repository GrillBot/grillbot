using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Models;

namespace GrillBot.App.Actions.Api.V1.Invite;

public class GetMetadataCount : ApiAction
{
    private InviteManager InviteManager { get; }

    public GetMetadataCount(ApiRequestContext apiContext, InviteManager inviteManager) : base(apiContext)
    {
        InviteManager = inviteManager;
    }

    public async Task<int> ProcessAsync()
        => await InviteManager.GetMetadataCountAsync();
}
