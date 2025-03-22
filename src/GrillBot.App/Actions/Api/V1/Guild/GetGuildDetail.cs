using AutoMapper;
using GrillBot.App.Managers.DataResolve;
using GrillBot.Cache.Services;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.Emote;
using GrillBot.Core.Services.PointsService;
using GrillBot.Core.Services.UserMeasures;
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
    private IPointsServiceClient PointsServiceClient { get; }
    private IAuditLogServiceClient AuditLogServiceClient { get; }
    private IUserMeasuresServiceClient UserMeasuresService { get; }

    private readonly DataResolveManager _dataResolve;
    private readonly IEmoteServiceClient _emoteService;

    public GetGuildDetail(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, IDiscordClient discordClient, GrillBotCacheBuilder cacheBuilder,
        ITextsManager texts, IPointsServiceClient pointsServiceClient, IAuditLogServiceClient auditLogServiceClient, IUserMeasuresServiceClient userMeasuresService,
        DataResolveManager dataResolve,
        IEmoteServiceClient emoteService) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        DiscordClient = discordClient;
        CacheBuilder = cacheBuilder;
        Texts = texts;
        PointsServiceClient = pointsServiceClient;
        AuditLogServiceClient = auditLogServiceClient;
        UserMeasuresService = userMeasuresService;
        _dataResolve = dataResolve;
        _emoteService = emoteService;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var id = (ulong)Parameters[0]!;

        using var repository = DatabaseBuilder.CreateRepository();

        var dbGuild = await repository.Guild.FindGuildByIdAsync(id, true)
            ?? throw new NotFoundException(Texts["GuildModule/GuildDetail/NotFound", ApiContext.Language]);

        var detail = Mapper.Map<GuildDetail>(dbGuild);
        var discordGuild = await DiscordClient.GetGuildAsync(id);
        if (discordGuild == null)
            return ApiResult.Ok(detail);

        detail.DatabaseReport = await CreateDatabaseReportAsync(id);
        detail = Mapper.Map(discordGuild, detail);
        if (!string.IsNullOrEmpty(dbGuild.AdminChannelId))
            detail.AdminChannel = await _dataResolve.GetChannelAsync(discordGuild.Id, dbGuild.AdminChannelId.ToUlong());

        if (!string.IsNullOrEmpty(dbGuild.BoosterRoleId))
            detail.BoosterRole = await _dataResolve.GetRoleAsync(dbGuild.BoosterRoleId.ToUlong());

        if (!string.IsNullOrEmpty(dbGuild.MuteRoleId))
            detail.MutedRole = await _dataResolve.GetRoleAsync(dbGuild.MuteRoleId.ToUlong());

        if (!string.IsNullOrEmpty(dbGuild.VoteChannelId))
            detail.VoteChannel = await _dataResolve.GetChannelAsync(discordGuild.Id, dbGuild.VoteChannelId.ToUlong());

        if (!string.IsNullOrEmpty(dbGuild.BotRoomChannelId))
            detail.BotRoomChannel = await _dataResolve.GetChannelAsync(discordGuild.Id, dbGuild.BotRoomChannelId.ToUlong());

        if (!string.IsNullOrEmpty(dbGuild.AssociationRoleId))
            detail.AssociationRole = await _dataResolve.GetRoleAsync(dbGuild.AssociationRoleId.ToUlong());

        var guildUsers = await discordGuild.GetUsersAsync();
        detail.UserStatusReport = guildUsers
            .GroupBy(o => o.GetStatus())
            .ToDictionary(o => o.Key, o => o.Count());

        detail.ClientTypeReport = guildUsers
            .Where(o => o.Status != UserStatus.Offline && o.Status != UserStatus.Invisible)
            .SelectMany(o => o.ActiveClients)
            .GroupBy(o => o)
            .ToDictionary(o => o.Key, o => o.Count());

        return ApiResult.Ok(detail);
    }

    private async Task<GuildDatabaseReport> CreateDatabaseReportAsync(ulong guildId)
    {
        using var repository = DatabaseBuilder.CreateRepository();

        var report = await repository.Guild.GetDatabaseReportDataAsync(guildId);

        using (var cache = CacheBuilder.CreateRepository())
            report.CacheIndexes = await cache.MessageIndexRepository.GetMessagesCountAsync(guildId: guildId);

        report.AuditLogs = await AuditLogServiceClient.GetItemsCountOfGuildAsync(guildId);
        report.PointTransactions = await PointsServiceClient.GetTransactionsCountForGuildAsync(guildId.ToString());
        report.UserMeasures = await UserMeasuresService.GetItemsCountOfGuildAsync(guildId.ToString());
        report.EmoteStats = await _emoteService.GetStatisticsCountInGuildAsync(guildId.ToString());

        return report;
    }
}
