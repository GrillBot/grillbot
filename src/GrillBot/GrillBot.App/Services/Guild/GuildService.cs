using GrillBot.App.Infrastructure;
using GrillBot.Data.Models.API.Guilds;

namespace GrillBot.App.Services;

public class GuildService : ServiceBase
{
    public GuildService(DiscordSocketClient client, GrillBotContextFactory dbFactory)
        : base(client, dbFactory)
    {
    }

    public async Task<GuildDetail> GetGuildDetailAsync(ulong id, CancellationToken cancellationToken = default)
    {
        using var dbContext = DbFactory.Create();

        var dbGuild = await dbContext.Guilds.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id.ToString(), cancellationToken);
        if (dbGuild == null) return null;

        var discordGuild = DiscordClient.GetGuild(id);
        var databaseReport = await CreateDatabaseReportAsync(id, cancellationToken);
        return new GuildDetail(discordGuild, dbGuild, databaseReport);
    }

    private async Task<GuildDatabaseReport> CreateDatabaseReportAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        using var dbContext = DbFactory.Create();

        var query = dbContext.Guilds.Where(o => o.Id == guildId.ToString()).Select(guild => new GuildDatabaseReport()
        {
            AuditLogs = guild.AuditLogs.Count,
            Channels = guild.Channels.Count,
            Invites = guild.Invites.Count,
            Searches = guild.Searches.Count,
            Unverifies = guild.Unverifies.Count,
            UnverifyLogs = guild.UnverifyLogs.Count,
            Users = guild.Users.Count,
        });

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
}
