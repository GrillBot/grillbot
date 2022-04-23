using AutoMapper;
using GrillBot.App.Infrastructure;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Guilds;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GrillBot.App.Services.Guild;

public class GuildApiService : ServiceBase
{
    public GuildApiService(GrillBotContextFactory dbFactory, DiscordSocketClient client, IMapper mapper) : base(client, dbFactory, null, null, mapper)
    {
    }

    public async Task<PaginatedResponse<Data.Models.API.Guilds.Guild>> GetListAsync(GetGuildListParams parameters, CancellationToken cancellationToken = default)
    {
        using var context = DbFactory.Create();

        var query = context.CreateQuery(parameters, true);
        var result = await PaginatedResponse<Data.Models.API.Guilds.Guild>
            .CreateAsync(query, parameters.Pagination, entity => Mapper.Map<Data.Models.API.Guilds.Guild>(entity), cancellationToken);

        for (int i = 0; i < result.Data.Count; i++)
        {
            var guild = DiscordClient.GetGuild(Convert.ToUInt64(result.Data[i].Id));
            if (guild == null) continue;

            result.Data[i] = Mapper.Map(guild, result.Data[i]);
        }

        return result;
    }

    public async Task<GuildDetail> GetDetailAsync(ulong id, CancellationToken cancellationToken = default)
    {
        using var context = DbFactory.Create();

        var dbGuild = await context.Guilds.FirstOrDefaultAsync(o => o.Id == id.ToString(), cancellationToken);
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
            CacheIndexes = dbContext.MessageCacheIndexes.Count(o => o.GuildId == guild.Id)
        });

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<GuildDetail> UpdateGuildAsync(ulong id, UpdateGuildParams parameters, ModelStateDictionary modelState)
    {
        var guild = DiscordClient.GetGuild(id);

        if (guild == null)
            return null;

        using var context = DbFactory.Create();

        var dbGuild = await context.Guilds.AsQueryable()
            .FirstOrDefaultAsync(o => o.Id == id.ToString());

        if (parameters.AdminChannelId != null && guild.GetTextChannel(Convert.ToUInt64(parameters.AdminChannelId)) == null)
            modelState.AddModelError(nameof(parameters.AdminChannelId), "Nepodařilo se dohledat administrátorský kanál");
        else
            dbGuild.AdminChannelId = parameters.AdminChannelId;

        if (parameters.MuteRoleId != null && guild.GetRole(Convert.ToUInt64(parameters.MuteRoleId)) == null)
            modelState.AddModelError(nameof(parameters.MuteRoleId), "Nepodařilo se dohledat roli, která reprezentuje umlčení uživatele při unverify.");
        else
            dbGuild.MuteRoleId = parameters.MuteRoleId;

        if (parameters.EmoteSuggestionChannelId != null && guild.GetTextChannel(Convert.ToUInt64(parameters.EmoteSuggestionChannelId)) == null)
            modelState.AddModelError(nameof(parameters.EmoteSuggestionChannelId), "Nepodařilo se dohledat kanál pro návrhy emotů.");
        else
            dbGuild.EmoteSuggestionChannelId = parameters.EmoteSuggestionChannelId;

        await context.SaveChangesAsync();
        return await GetDetailAsync(id);
    }
}
