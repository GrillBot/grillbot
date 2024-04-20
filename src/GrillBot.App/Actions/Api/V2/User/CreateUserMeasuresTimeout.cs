using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;

namespace GrillBot.App.Actions.Api.V2.User;

public class CreateUserMeasuresTimeout : ApiAction
{
    public CreateUserMeasuresTimeout(ApiRequestContext apiContext) : base(apiContext)
    {
    }

    public override Task<ApiResult> ProcessAsync()
    {
        return Task.FromResult(ApiResult.Ok());
    }
}
