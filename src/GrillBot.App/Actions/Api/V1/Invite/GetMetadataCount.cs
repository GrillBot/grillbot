using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Core.Services.InviteService;
using GrillBot.Core.Services.InviteService.Models.Request;

namespace GrillBot.App.Actions.Api.V1.Invite;

public class GetMetadataCount(
    ApiRequestContext apiContext,
    IServiceClientExecutor<IInviteServiceClient> _client
) : ApiAction(apiContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var request = new InviteListRequest();
        var response = await _client.ExecuteRequestAsync((client, cancellationToken) => client.GetCachedInvitesAsync(request, cancellationToken));

        return ApiResult.Ok(response.TotalItemsCount);
    }
}
