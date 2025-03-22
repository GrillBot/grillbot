using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Infrastructure.Actions;

namespace GrillBot.App.Actions.Api.V1.Invite;

public class DeleteInvite : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }

    public DeleteInvite(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var guildId = (ulong)Parameters[0]!;
        var code = (string)Parameters[1]!;

        using var repository = DatabaseBuilder.CreateRepository();

        var invite = await repository.Invite.FindInviteByCodeAsync(guildId, code)
            ?? throw new NotFoundException(Texts["Invite/NotFound", ApiContext.Language].FormatWith(code));

        var users = await repository.GuildUser.FindUsersWithInviteCode(guildId, code);
        foreach (var user in users)
            user.UsedInviteCode = null;

        repository.Remove(invite);
        await repository.CommitAsync();

        return ApiResult.Ok();
    }
}
