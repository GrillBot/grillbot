using AutoMapper;
using GrillBot.Cache.Services;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Extensions;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Database.Models.Guilds;

namespace GrillBot.App.Actions.Api.V1.Guild;

public class GetGuildDetail : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }
    private IDiscordClient DiscordClient { get; }
    private GrillBotCacheBuilder CacheBuilder { get; }
    private ITextsManager Texts { get; }

    public GetGuildDetail(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, IDiscordClient discordClient, GrillBotCacheBuilder cacheBuilder,
        ITextsManager texts) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        DiscordClient = discordClient;
        CacheBuilder = cacheBuilder;
        Texts = texts;
    }

    public async Task<GuildDetail> ProcessAsync(ulong id)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var dbGuild = await repository.Guild.FindGuildByIdAsync(id, true);
        if (dbGuild == null)
            throw new NotFoundException(Texts["GuildModule/GuildDetail/NotFound", ApiContext.Language]);

        var detail = Mapper.Map<GuildDetail>(dbGuild);
        var discordGuild = await DiscordClient.GetGuildAsync(id);
        if (discordGuild == null) return detail;

        detail.DatabaseReport = await CreateDatabaseReportAsync(id);
        detail = Mapper.Map(discordGuild, detail);
        if (!string.IsNullOrEmpty(dbGuild.AdminChannelId))
            detail.AdminChannel = Mapper.Map<Data.Models.API.Channels.Channel>(await discordGuild.GetChannelAsync(dbGuild.AdminChannelId.ToUlong()));

        if (!string.IsNullOrEmpty(dbGuild.EmoteSuggestionChannelId))
            detail.EmoteSuggestionChannel = Mapper.Map<Data.Models.API.Channels.Channel>(await discordGuild.GetChannelAsync(dbGuild.EmoteSuggestionChannelId.ToUlong()));

        if (!string.IsNullOrEmpty(dbGuild.BoosterRoleId))
            detail.BoosterRole = Mapper.Map<Role>(discordGuild.GetRole(dbGuild.BoosterRoleId.ToUlong()));

        if (!string.IsNullOrEmpty(dbGuild.MuteRoleId))
            detail.MutedRole = Mapper.Map<Role>(discordGuild.GetRole(dbGuild.MuteRoleId.ToUlong()));

        if (!string.IsNullOrEmpty(dbGuild.VoteChannelId))
            detail.VoteChannel = Mapper.Map<Data.Models.API.Channels.Channel>(await discordGuild.GetChannelAsync(dbGuild.VoteChannelId.ToUlong()));

        if (!string.IsNullOrEmpty(dbGuild.BotRoomChannelId))
            detail.BotRoomChannel = Mapper.Map<Data.Models.API.Channels.Channel>(await discordGuild.GetChannelAsync(dbGuild.BotRoomChannelId.ToUlong()));

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
        var report = await repository.Guild.GetDatabaseReportDataAsync(guildId);

        await using var cache = CacheBuilder.CreateRepository();
        report.CacheIndexes = await cache.MessageIndexRepository.GetMessagesCountAsync(guildId: guildId);

        return report;
    }
}
