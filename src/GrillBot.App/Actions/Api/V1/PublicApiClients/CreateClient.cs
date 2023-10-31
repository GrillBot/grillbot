using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Models.API.ApiClients;
using GrillBot.Database.Entity;

namespace GrillBot.App.Actions.Api.V1.PublicApiClients;

public class CreateClient : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public CreateClient(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = (ApiClientParams)Parameters[0]!;

        var entity = new ApiClient
        {
            Id = Guid.NewGuid().ToString(),
            AllowedMethods = parameters.AllowedMethods,
            Name = parameters.Name,
            Disabled = parameters.Disabled
        };

        await using var repository = DatabaseBuilder.CreateRepository();

        await repository.AddAsync(entity);
        await repository.CommitAsync();
        return ApiResult.Ok();
    }
}
