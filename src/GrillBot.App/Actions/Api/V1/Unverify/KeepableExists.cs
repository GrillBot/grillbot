using GrillBot.Common.Models;
using GrillBot.Data.Models.API.Selfunverify;

namespace GrillBot.App.Actions.Api.V1.Unverify;

public class KeepableExists : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public KeepableExists(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task<bool> ProcessAsync(KeepableParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        return await repository.SelfUnverify.KeepableExistsAsync(parameters.Group, parameters.Name);
    }
}
