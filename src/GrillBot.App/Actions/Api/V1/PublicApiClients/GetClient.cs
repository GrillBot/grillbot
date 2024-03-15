using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Infrastructure.Actions;

namespace GrillBot.App.Actions.Api.V1.PublicApiClients;

public class GetClient : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public GetClient(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var clientId = (string)Parameters[0]!;

        await using var repository = DatabaseBuilder.CreateRepository();

        var client = await repository.ApiClientRepository.FindClientById(clientId)
            ?? throw new NotFoundException();

        return ApiResult.Ok(client);
    }
}
