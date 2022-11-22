using GrillBot.Common.Extensions;

namespace GrillBot.App.Services.Unverify;

public class UnverifyHelper
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public UnverifyHelper(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task<IRole> GetMuteRoleAsync(IGuild guild)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var guildData = await repository.Guild.FindGuildAsync(guild, true);
        return string.IsNullOrEmpty(guildData?.MuteRoleId) ? null : guild.GetRole(guildData.MuteRoleId.ToUlong());
    }
}
