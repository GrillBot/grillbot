using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Data.Exceptions;

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

    public async Task ProcessAsync(ulong guildId, string code)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var invite = await repository.Invite.FindInviteByCodeAsync(guildId, code);
        if (invite == null)
            throw new NotFoundException(Texts["Invite/NotFound", ApiContext.Language].FormatWith(code));

        var users = await repository.GuildUser.FindUsersWithInviteCode(guildId, code);
        foreach (var user in users) user.UsedInviteCode = null;

        repository.Remove(invite);
        await repository.CommitAsync();
    }
}
