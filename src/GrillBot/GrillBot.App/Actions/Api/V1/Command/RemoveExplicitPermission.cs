using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Data.Exceptions;

namespace GrillBot.App.Actions.Api.V1.Command;

public class RemoveExplicitPermission : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }

    public RemoveExplicitPermission(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
    }

    public async Task ProcessAsync(string command, string targetId)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var permission = await repository.Permissions.FindPermissionForTargetAsync(command, targetId);
        if (permission == null)
            throw new NotFoundException(Texts["ExplicitPerms/Remove/NotFound", ApiContext.Language].FormatWith(command, targetId));

        repository.Remove(permission);
        await repository.CommitAsync();
    }
}
