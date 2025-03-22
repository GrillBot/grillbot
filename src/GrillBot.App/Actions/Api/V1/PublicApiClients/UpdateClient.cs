using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Models.API.ApiClients;

namespace GrillBot.App.Actions.Api.V1.PublicApiClients;

public class UpdateClient : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }

    public UpdateClient(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var id = (string)Parameters[0]!;
        var parameters = (ApiClientParams)Parameters[1]!;

        using var repository = DatabaseBuilder.CreateRepository();

        var apiClient = await repository.ApiClientRepository.FindClientById(id)
            ?? throw new NotFoundException(Texts["PublicApiClients/NotFound", ApiContext.Language]);

        apiClient.AllowedMethods = parameters.AllowedMethods;
        apiClient.Name = parameters.Name;
        apiClient.Disabled = parameters.Disabled;
        await repository.CommitAsync();
        return ApiResult.Ok();
    }
}
