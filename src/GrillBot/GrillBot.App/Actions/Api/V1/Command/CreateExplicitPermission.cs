using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.Permissions;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Actions.Api.V1.Command;

public class CreateExplicitPermission : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }

    public bool IsConflict { get; private set; }
    public string ErrorMessage { get; private set; }

    public CreateExplicitPermission(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
    }

    public async Task ProcessAsync(CreateExplicitPermissionParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        await CheckConflictAsync(parameters, repository);
        if (IsConflict) return;

        if (!char.IsLetter(parameters.Command[0]))
            parameters.Command = parameters.Command[1..];

        var permission = new Database.Entity.ExplicitPermission
        {
            IsRole = parameters.IsRole,
            TargetId = parameters.TargetId,
            Command = parameters.Command,
            State = parameters.State
        };

        await repository.AddAsync(permission);
        await repository.CommitAsync();
    }

    private async Task CheckConflictAsync(CreateExplicitPermissionParams parameters, GrillBotRepository repository)
    {
        IsConflict = await repository.Permissions.ExistsCommandForTargetAsync(parameters.Command, parameters.TargetId);

        if (IsConflict)
            ErrorMessage = Texts["ExplicitPerms/Create/Conflict", ApiContext.Language].FormatWith(parameters.Command, parameters.TargetId);
    }
}
