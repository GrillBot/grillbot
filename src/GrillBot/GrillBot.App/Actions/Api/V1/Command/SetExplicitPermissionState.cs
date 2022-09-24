using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Data.Exceptions;
using GrillBot.Database.Enums;

namespace GrillBot.App.Actions.Api.V1.Command;

public class SetExplicitPermissionState : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }

    public SetExplicitPermissionState(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
    }

    public async Task ProcessAsync(string command, string targetId, ExplicitPermissionState state)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var permission = await repository.Permissions.FindPermissionForTargetAsync(command, targetId);
        if (permission == null)
            throw new NotFoundException(Texts["ExplicitPerms/NotFound", ApiContext.Language].FormatWith(command, targetId));

        permission.State = state;
        await repository.CommitAsync();
    }
}
