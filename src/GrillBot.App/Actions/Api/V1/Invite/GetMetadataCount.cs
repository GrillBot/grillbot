using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;

namespace GrillBot.App.Actions.Api.V1.Invite;

public class GetMetadataCount : ApiAction
{
    private InviteManager InviteManager { get; }

    public GetMetadataCount(ApiRequestContext apiContext, InviteManager inviteManager) : base(apiContext)
    {
        InviteManager = inviteManager;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var result = await InviteManager.GetMetadataCountAsync();
        return ApiResult.Ok(result);
    }
}
