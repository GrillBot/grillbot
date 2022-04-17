using AutoMapper;
using GrillBot.App.Infrastructure;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.API.Guilds;

namespace GrillBot.App.Services;

public class GuildService : ServiceBase
{
    public GuildService(DiscordSocketClient client, GrillBotContextFactory dbFactory, IMapper mapper)
        : base(client, dbFactory, mapper: mapper)
    {
    }

    public async Task<Database.Entity.Guild> GetGuildAsync(ulong id, CancellationToken cancellationToken = default)
    {
        using var context = DbFactory.Create();

        return await context.Guilds.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id.ToString(), cancellationToken);
    }

    public async Task<GuildDetail> GetGuildDetailAsync(ulong id, CancellationToken cancellationToken = default)
    {
        var dbGuild = await GetGuildAsync(id, cancellationToken);
        if (dbGuild == null) return null;

        var detail = Mapper.Map<GuildDetail>(dbGuild);
        detail.DatabaseReport = await CreateDatabaseReportAsync(id, cancellationToken);

        var discordGuild = DiscordClient.GetGuild(id);
        if (discordGuild != null)
        {
            detail = Mapper.Map(discordGuild, detail);

            if (!string.IsNullOrEmpty(dbGuild.AdminChannelId))
                detail.AdminChannel = Mapper.Map<Channel>(discordGuild.GetChannel(Convert.ToUInt64(dbGuild.AdminChannelId)));

            if (!string.IsNullOrEmpty(dbGuild.EmoteSuggestionChannelId))
                detail.EmoteSuggestionChannel = Mapper.Map<Channel>(discordGuild.GetChannel(Convert.ToUInt64(dbGuild.EmoteSuggestionChannelId)));

            if (!string.IsNullOrEmpty(dbGuild.BoosterRoleId))
                detail.BoosterRole = Mapper.Map<Role>(discordGuild.GetRole(Convert.ToUInt64(dbGuild.BoosterRoleId)));

            if (!string.IsNullOrEmpty(dbGuild.MuteRoleId))
                detail.MutedRole = Mapper.Map<Role>(discordGuild.GetRole(Convert.ToUInt64(dbGuild.MuteRoleId)));

            detail.UserStatusReport = discordGuild.Users.GroupBy(o =>
            {
                if (o.Status == UserStatus.AFK) return UserStatus.Idle;
                else if (o.Status == UserStatus.Invisible) return UserStatus.Offline;
                return o.Status;
            }).ToDictionary(o => o.Key, o => o.Count());

            detail.ClientTypeReport = discordGuild.Users
                .Where(o => o.Status != UserStatus.Offline && o.Status != UserStatus.Invisible)
                .SelectMany(o => o.ActiveClients)
                .GroupBy(o => o)
                .ToDictionary(o => o.Key, o => o.Count());
        }

        return detail;
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
