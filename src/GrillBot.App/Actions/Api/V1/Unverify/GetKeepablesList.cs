using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.Common.Executor;
using UnverifyService;
using UnverifyService.Models.Request.Keepables;

namespace GrillBot.App.Actions.Api.V1.Unverify;

public class GetKeepablesList(
    ApiRequestContext apiContext,
    GrillBotDatabaseBuilder databaseBuilder,
    IServiceClientExecutor<IUnverifyServiceClient> unverifyClient
) : ApiAction(apiContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var group = (string?)Parameters[0];

        var request = new KeepablesListRequest
        {
            Group = (string?)Parameters[0],
            Pagination = new Core.Models.Pagination.PaginatedParams
            {
                Page = 0,
                PageSize = int.MaxValue
            }
        };

        var data = await unverifyClient.ExecuteRequestAsync(
            async (client, ctx) => await client.GetKeepablesListAsync(request, ctx.CancellationToken),
            CancellationToken
        );

        var result = data.Data
            .GroupBy(o => o.Group.ToUpper())
            .ToDictionary(
                o => o.Key,
                o => o.Select(x => x.Name.ToUpper()).ToList()
            );

        return ApiResult.Ok(result);
    }

    public async Task<Dictionary<string, List<string>>> ProcessAsync(string? group)
    {
        using var repository = databaseBuilder.CreateRepository();

        var items = await repository.SelfUnverify.GetKeepablesAsync(group);
        return items.GroupBy(o => o.GroupName.ToUpper())
            .ToDictionary(o => o.Key, o => o.Select(x => x.Name.ToUpper()).ToList());
    }
}
