using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
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

    public async Task ProcessAsync(string id, ApiClientParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var apiClient = await repository.ApiClientRepository.FindClientById(id);
        if (apiClient == null)
            throw new NotFoundException(Texts["PublicApiClients/NotFound", ApiContext.Language]);

        apiClient.AllowedMethods = parameters.AllowedMethods;
        apiClient.Name = parameters.Name;
        apiClient.Disabled = parameters.Disabled;
        await repository.CommitAsync();
    }
}
