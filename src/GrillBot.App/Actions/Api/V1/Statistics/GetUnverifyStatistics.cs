using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.Common.Executor;
using UnverifyService;
using UnverifyService.Core.Enums;

namespace GrillBot.App.Actions.Api.V1.Statistics;

public class GetUnverifyStatistics(
    ApiRequestContext apiContext,
    IServiceClientExecutor<IUnverifyServiceClient> _unverifyClient
) : ApiAction(apiContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        return (string)Parameters[0]! switch
        {
            "ByOperation" => ApiResult.Ok(new Dictionary<string, int>()),
            "ByDate" => ApiResult.Ok(await ProcessByDateAsync()),
            _ => ApiResult.BadRequest(),
        };
    }

    private async Task<Dictionary<string, long>> ProcessByDateAsync()
    {
        var result = new Dictionary<string, long>();

        foreach (var type in Enum.GetValues<UnverifyOperationType>())
        {
            var statistics = await _unverifyClient.ExecuteRequestAsync(
                async (client, ctx) => await client.GetPeriodStatisticsAsync("ByMonth", type, ctx.CancellationToken),
                CancellationToken
            );

            foreach (var item in statistics)
            {
                if (!result.TryGetValue(item.Key, out var count))
                    result.Add(item.Key, item.Value);
                else
                    result[item.Key] = count + item.Value;
            }
        }

        return result;
    }
}
