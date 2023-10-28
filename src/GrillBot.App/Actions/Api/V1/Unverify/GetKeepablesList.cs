using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;

namespace GrillBot.App.Actions.Api.V1.Unverify;

public class GetKeepablesList : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public GetKeepablesList(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var group = (string?)Parameters[0];

        await using var repository = DatabaseBuilder.CreateRepository();

        var items = await repository.SelfUnverify.GetKeepablesAsync(group);
        var result = items.GroupBy(o => o.GroupName.ToUpper())
            .ToDictionary(o => o.Key, o => o.Select(x => x.Name.ToUpper()).ToList());

        return ApiResult.Ok(result);
    }
}
