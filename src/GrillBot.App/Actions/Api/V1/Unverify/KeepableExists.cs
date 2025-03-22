using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Models.API.Selfunverify;

namespace GrillBot.App.Actions.Api.V1.Unverify;

public class KeepableExists : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public KeepableExists(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = (KeepableParams)Parameters[0]!;

        using var repository = DatabaseBuilder.CreateRepository();

        var result = await repository.SelfUnverify.KeepableExistsAsync(parameters.Group, parameters.Name);
        return ApiResult.Ok(result);
    }
}
