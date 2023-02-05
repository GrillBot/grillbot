using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Localization;

namespace GrillBot.App.Helpers;

public class UnverifyHelper
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public UnverifyHelper(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task<IRole?> GetMuteRoleAsync(IGuild guild)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var guildData = await repository.Guild.FindGuildAsync(guild, true);
        return string.IsNullOrEmpty(guildData?.MuteRoleId) ? null : guild.GetRole(guildData.MuteRoleId.ToUlong());
    }

    public async Task<string> GetUserLanguageAsync(IGuildUser user, string commandLanguage, bool selfunverify)
    {
        if (selfunverify) return commandLanguage;

        await using var repository = DatabaseBuilder.CreateRepository();
        var userEntity = await repository.User.FindUserAsync(user, true);
        return userEntity?.Language ?? TextsManager.DefaultLocale;
    }
}
