using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Data.Exceptions;

namespace GrillBot.App.Actions.Api.V1.PublicApiClients;

public class DeleteClient : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }

    public DeleteClient(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
    }

    public async Task ProcessAsync(string id)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var apiClient = await repository.ApiClientRepository.FindClientById(id);
        if (apiClient == null)
            throw new NotFoundException(Texts["PublicApiClients/NotFound", ApiContext.Language]);

        repository.Remove(apiClient);
        await repository.CommitAsync();
    }
}
