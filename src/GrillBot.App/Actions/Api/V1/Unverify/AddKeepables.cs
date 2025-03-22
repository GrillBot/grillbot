using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Models.API.Selfunverify;
using GrillBot.Database.Entity;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Actions.Api.V1.Unverify;

public class AddKeepables : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }

    public AddKeepables(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = (List<KeepableParams>)Parameters[0]!;
        using var repository = DatabaseBuilder.CreateRepository();
        await ValidateParameters(parameters, repository);

        var entities = parameters.ConvertAll(o => new SelfunverifyKeepable
        {
            Name = o.Name.ToLower(),
            GroupName = o.Group.ToLower()
        });

        await repository.AddCollectionAsync(entities);
        await repository.CommitAsync();
        return ApiResult.Ok();
    }

    private async Task ValidateParameters(List<KeepableParams> parameters, GrillBotRepository repository)
    {
        foreach (var param in parameters)
            await ValidateParameters(param, repository);
    }

    private async Task ValidateParameters(KeepableParams parameter, GrillBotRepository repository)
    {
        if (!await repository.SelfUnverify.KeepableExistsAsync(parameter.Group, parameter.Name)) return;

        throw new ValidationException(
            new ValidationResult(Texts["Unverify/SelfUnverify/Keepables/Exists", ApiContext.Language].FormatWith(parameter.Group, parameter.Name),
                new[] { nameof(parameter.Group), nameof(parameter.Name) }), null, parameter
        );
    }
}
