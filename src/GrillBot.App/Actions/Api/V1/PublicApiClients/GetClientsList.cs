using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;

namespace GrillBot.App.Actions.Api.V1.PublicApiClients;

public class GetClientsList : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public GetClientsList(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var result = await repository.ApiClientRepository.GetClientsAsync();

        return ApiResult.Ok(result);
    }
}
