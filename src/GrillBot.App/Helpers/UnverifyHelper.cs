using GrillBot.Core.Extensions;

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
        using var repository = DatabaseBuilder.CreateRepository();

        var guildData = await repository.Guild.FindGuildAsync(guild, true);
        return string.IsNullOrEmpty(guildData?.MuteRoleId) ? null : guild.GetRole(guildData.MuteRoleId.ToUlong());
    }
}
