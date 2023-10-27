using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Infrastructure.Actions;

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

    public override async Task<ApiResult> ProcessAsync()
    {
        var id = (string)Parameters[0]!;

        await using var repository = DatabaseBuilder.CreateRepository();

        var apiClient = await repository.ApiClientRepository.FindClientById(id)
            ?? throw new NotFoundException(Texts["PublicApiClients/NotFound", ApiContext.Language]);

        repository.Remove(apiClient);
        await repository.CommitAsync();
        return ApiResult.Ok();
    }
}
