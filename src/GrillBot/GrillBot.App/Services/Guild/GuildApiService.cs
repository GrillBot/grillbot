using AutoMapper;
using GrillBot.Cache.Services;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Database.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GrillBot.App.Services.Guild;

public class GuildApiService
{
    private IDiscordClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }
    private GrillBotCacheBuilder CacheBuilder { get; }

    public GuildApiService(GrillBotDatabaseBuilder databaseBuilder, IDiscordClient client, IMapper mapper,
        GrillBotCacheBuilder cacheBuilder)
    {
        DatabaseBuilder = databaseBuilder;
        DiscordClient = client;
        Mapper = mapper;
        CacheBuilder = cacheBuilder;
    }

    public async Task<PaginatedResponse<Data.Models.API.Guilds.Guild>> GetListAsync(GetGuildListParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.Guild.GetGuildListAsync(parameters, parameters.Pagination);
        var result = await PaginatedResponse<Data.Models.API.Guilds.Guild>
            .CopyAndMapAsync(data, entity => Task.FromResult(Mapper.Map<Data.Models.API.Guilds.Guild>(entity)));

        for (var i = 0; i < result.Data.Count; i++)
        {
            var guild = await DiscordClient.GetGuildAsync(result.Data[i].Id.ToUlong());
            if (guild == null) continue;

            result.Data[i] = Mapper.Map(guild, result.Data[i]);
        }

        return result;
    }

    public async Task<GuildDetail> GetDetailAsync(ulong id)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var dbGuild = await repository.Guild.FindGuildByIdAsync(id);
        if (dbGuild == null) return null;

        var detail = Mapper.Map<GuildDetail>(dbGuild);
        detail.DatabaseReport = await CreateDatabaseReportAsync(id);

        var discordGuild = await DiscordClient.GetGuildAsync(id);
        if (discordGuild == null)
            return detail;

        detail = Mapper.Map(discordGuild, detail);
        if (!string.IsNullOrEmpty(dbGuild.AdminChannelId))
            detail.AdminChannel = Mapper.Map<Channel>(await discordGuild.GetChannelAsync(dbGuild.AdminChannelId.ToUlong()));

        if (!string.IsNullOrEmpty(dbGuild.EmoteSuggestionChannelId))
            detail.EmoteSuggestionChannel = Mapper.Map<Channel>(await discordGuild.GetChannelAsync(dbGuild.EmoteSuggestionChannelId.ToUlong()));

        if (!string.IsNullOrEmpty(dbGuild.BoosterRoleId))
            detail.BoosterRole = Mapper.Map<Role>(discordGuild.GetRole(dbGuild.BoosterRoleId.ToUlong()));

        if (!string.IsNullOrEmpty(dbGuild.MuteRoleId))
            detail.MutedRole = Mapper.Map<Role>(discordGuild.GetRole(dbGuild.MuteRoleId.ToUlong()));

        if (!string.IsNullOrEmpty(dbGuild.VoteChannelId))
            detail.VoteChannel = Mapper.Map<Channel>(await discordGuild.GetChannelAsync(dbGuild.VoteChannelId.ToUlong()));

        var guildUsers = await discordGuild.GetUsersAsync();
        detail.UserStatusReport = guildUsers
            .GroupBy(o => o.GetStatus())
            .ToDictionary(o => o.Key, o => o.Count());

        detail.ClientTypeReport = guildUsers
            .Where(o => o.Status != UserStatus.Offline && o.Status != UserStatus.Invisible)
            .SelectMany(o => o.ActiveClients)
            .GroupBy(o => o)
            .ToDictionary(o => o.Key, o => o.Count());

        return detail;
    }

    private async Task<GuildDatabaseReport> CreateDatabaseReportAsync(ulong guildId)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var data = await repository.Guild.GetDatabaseReportDataAsync(guildId);

        var report = new GuildDatabaseReport
        {
            Channels = data.channels,
            Invites = data.invites,
            Searches = data.searches,
            Unverifies = data.unverify,
            Users = data.users,
            AuditLogs = data.auditLogs,
            UnverifyLogs = data.unverifyLogs
        };

        await using var cache = CacheBuilder.CreateRepository();
        report.CacheIndexes = await cache.MessageIndexRepository.GetMessagesCountAsync(guildId: guildId);

        return report;
    }

    public async Task<GuildDetail> UpdateGuildAsync(ulong id, UpdateGuildParams parameters, ModelStateDictionary modelState)
    {
        var guild = await DiscordClient.GetGuildAsync(id);
        if (guild == null)
            return null;

        await using var repository = DatabaseBuilder.CreateRepository();

        var dbGuild = await repository.Guild.GetOrCreateRepositoryAsync(guild);
        if (!string.IsNullOrEmpty(parameters.AdminChannelId) && await guild.GetTextChannelAsync(parameters.AdminChannelId.ToUlong()) == null)
            modelState.AddModelError(nameof(parameters.AdminChannelId), "Nepodařilo se dohledat administrátorský kanál");
        else
            dbGuild.AdminChannelId = parameters.AdminChannelId;

        if (!string.IsNullOrEmpty(parameters.MuteRoleId) && guild.GetRole(parameters.MuteRoleId.ToUlong()) == null)
            modelState.AddModelError(nameof(parameters.MuteRoleId), "Nepodařilo se dohledat roli, která reprezentuje umlčení uživatele při unverify.");
        else
            dbGuild.MuteRoleId = parameters.MuteRoleId;

        if (!string.IsNullOrEmpty(parameters.EmoteSuggestionChannelId) && await guild.GetTextChannelAsync(parameters.EmoteSuggestionChannelId.ToUlong()) == null)
            modelState.AddModelError(nameof(parameters.EmoteSuggestionChannelId), "Nepodařilo se dohledat kanál pro návrhy emotů.");
        else
            dbGuild.EmoteSuggestionChannelId = parameters.EmoteSuggestionChannelId;

        if (!string.IsNullOrEmpty(parameters.VoteChannelId) && await guild.GetTextChannelAsync(parameters.VoteChannelId.ToUlong()) == null)
            modelState.AddModelError(nameof(parameters.VoteChannelId), "Nepodařilo se dohledat kanál pro veřejná hlasování.");
        else
            dbGuild.VoteChannelId = parameters.VoteChannelId;

        if (modelState.IsValid)
            await repository.CommitAsync();
        return await GetDetailAsync(id);
    }
}
