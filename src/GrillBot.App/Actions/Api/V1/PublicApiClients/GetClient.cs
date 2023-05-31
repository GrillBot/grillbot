using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Database.Entity;

namespace GrillBot.App.Actions.Api.V1.PublicApiClients;

public class GetClient : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    
    public GetClient(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task<ApiClient> ProcessAsync(string clientId)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var client = await repository.ApiClientRepository.FindClientById(clientId);
        return client ?? throw new NotFoundException();
    }
}
