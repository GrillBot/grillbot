using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.Common.Exceptions;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Data.Models.API.Selfunverify;
using Microsoft.AspNetCore.Mvc;
using UnverifyService;
using UnverifyService.Models.Request.Keepables;

namespace GrillBot.App.Actions.Api.V1.Unverify;

public class AddKeepables(
    ApiRequestContext apiContext,
    IServiceClientExecutor<IUnverifyServiceClient> _unverifyClient
) : ApiAction(apiContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = (List<KeepableParams>)Parameters[0]!;

        var requests = parameters.ConvertAll(o => new CreateKeepableRequest
        {
            Group = o.Group,
            Name = o.Name,
        });

        try
        {
            await _unverifyClient.ExecuteRequestAsync((client, ctx) => client.CreateKeepablesAsync(requests, ctx.CancellationToken), CancellationToken);
            return ApiResult.Ok();
        }
        catch (ClientBadRequestException ex)
        {
            return ApiResult.BadRequest(new ValidationProblemDetails(ex.ValidationErrors));
        }
    }
}
