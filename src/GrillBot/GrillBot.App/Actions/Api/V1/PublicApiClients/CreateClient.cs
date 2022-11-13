using GrillBot.Common.Models;
using GrillBot.Database.Entity;

namespace GrillBot.App.Actions.Api.V1.PublicApiClients;

public class CreateClient : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public CreateClient(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(List<string> allowedMethods)
    {
        var entity = new ApiClient
        {
            Id = Guid.NewGuid().ToString(),
            AllowedMethods = allowedMethods
        };

        await using var repository = DatabaseBuilder.CreateRepository();

        await repository.AddAsync(entity);
        await repository.CommitAsync();
    }
}
