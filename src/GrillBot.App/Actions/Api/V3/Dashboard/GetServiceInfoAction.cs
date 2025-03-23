using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.Common;
using GrillBot.Data.Models.API.System;
using Refit;

namespace GrillBot.App.Actions.Api.V3.Dashboard;

public class GetServiceInfoAction(
    ApiRequestContext apiContext,
    IServiceProvider _serviceProvider
) : ApiAction(apiContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var serviceId = GetParameter<string>(0);
        var client = GetClient(serviceId);
        var isAvailable = await IsServiceHealthyAsync(client);
        var uptime = await GetUptimeAsync(client);

        return ApiResult.Ok(new DashboardService(serviceId, isAvailable, uptime));
    }

    private IServiceClient? GetClient(string serviceId)
    {
        return typeof(IServiceClient).Assembly.GetTypes()
            .Where(o => o.IsInterface && o.GetInterface(nameof(IServiceClient)) is not null)
            .Select(_serviceProvider.GetService)
            .OfType<IServiceClient>()
            .FirstOrDefault(client =>
            {
                return client.GetType().GetInterfaces()
                    .Where(@interface => @interface.Name != nameof(IServiceClient))
                    .Select(@interface => typeof(SettingsFor<>).MakeGenericType(@interface))
                    .Select(@interface => (_serviceProvider.GetService(@interface) as ISettingsFor)?.Settings)
                    .Select(settings => settings?.HttpRequestMessageOptions?.TryGetValue("ServiceName", out var name) == true ? name?.ToString() : null)
                    .Any(name => !string.IsNullOrEmpty(name) && name == serviceId);
            });
    }

    private static async Task<long> GetUptimeAsync(IServiceClient? client)
        => client is null ? -1 : await client.GetUptimeAsync();

    private static async Task<bool> IsServiceHealthyAsync(IServiceClient? client)
    {
        try
        {
            if (client is null)
                return false;

            await client.IsHealthyAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
