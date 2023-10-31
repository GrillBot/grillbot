using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.Common;

namespace GrillBot.App.Actions.Api;

public class ServiceBridgeAction<TServiceClient> : ApiAction where TServiceClient : IClient
{
    private TServiceClient Client { get; }

    public ServiceBridgeAction(ApiRequestContext apiContext, TServiceClient client) : base(apiContext)
    {
        Client = client;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var funcExecutor = Parameters.OfType<Func<TServiceClient, Task<object>>>().FirstOrDefault();
        var actionExecutor = Parameters.OfType<Func<TServiceClient, Task>>().FirstOrDefault();

        if (funcExecutor is not null)
            return ApiResult.Ok(await funcExecutor(Client));

        if (actionExecutor is not null)
        {
            await actionExecutor(Client);
            return ApiResult.Ok();
        }

        return ApiResult.BadRequest();
    }
}
