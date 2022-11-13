using GrillBot.Common.Models;
using GrillBot.Database.Entity;

namespace GrillBot.App.Actions.Api.V1.PublicApiClients;

public class GetClientsList : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    
    public GetClientsList(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task<List<ApiClient>> ProcessAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        return await repository.ApiClientRepository.GetClientsAsync();
    }
}
