using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.Common;
using GrillBot.Data.Models.API.System;

namespace GrillBot.App.Actions.Api.V3.Dashboard;

public class GetServiceInfoAction : ApiAction
{
    private readonly IServiceProvider _serviceProvider;

    public GetServiceInfoAction(ApiRequestContext apiContext, IServiceProvider serviceProvider) : base(apiContext)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var serviceId = GetParameter<string>(0);
        var client = GetClient(serviceId);
        var isAvailable = client is not null && await client.IsHealthyAsync();
        var uptime = await GetUptimeAsync(client);
        var result = new DashboardService(serviceId, client?.ServiceName ?? serviceId, isAvailable, uptime);

        return ApiResult.Ok(result);
    }

    private IClient? GetClient(string serviceId)
    {
        return typeof(IClient).Assembly.GetTypes()
            .Where(o => o.IsInterface && o.GetInterface(nameof(IClient)) is not null)
            .Select(_serviceProvider.GetService)
            .OfType<IClient>()
            .FirstOrDefault(c => c.ServiceName == serviceId);
    }

    private static async Task<long> GetUptimeAsync(IClient? client)
        => client is null ? -1 : await client.GetUptimeAsync();
}
