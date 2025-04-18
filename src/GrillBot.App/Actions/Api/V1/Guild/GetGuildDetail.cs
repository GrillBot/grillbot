using AutoMapper;
using GrillBot.App.Managers.DataResolve;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Core.Services.Emote;
using GrillBot.Core.Services.InviteService;
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
    private ITextsManager Texts { get; }

    private readonly DataResolveManager _dataResolve;
    private readonly IServiceClientExecutor<IEmoteServiceClient> _emoteService;
    private readonly IServiceClientExecutor<IAuditLogServiceClient> _auditLogServiceClient;
    private readonly IServiceClientExecutor<IPointsServiceClient> _pointsServiceClient;
    private readonly IServiceClientExecutor<IUserMeasuresServiceClient> _userMeasuresService;
    private readonly IServiceClientExecutor<IInviteServiceClient> _inviteServiceClient;

    public GetGuildDetail(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, IDiscordClient discordClient, ITextsManager texts,
        IServiceClientExecutor<IPointsServiceClient> pointsServiceClient, IServiceClientExecutor<IAuditLogServiceClient> auditLogServiceClient,
        IServiceClientExecutor<IUserMeasuresServiceClient> userMeasuresService, DataResolveManager dataResolve, IServiceClientExecutor<IEmoteServiceClient> emoteService,
        IServiceClientExecutor<IInviteServiceClient> inviteServiceClient) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        DiscordClient = discordClient;
        Texts = texts;
        _pointsServiceClient = pointsServiceClient;
        _auditLogServiceClient = auditLogServiceClient;
        _userMeasuresService = userMeasuresService;
        _dataResolve = dataResolve;
        _emoteService = emoteService;
        _inviteServiceClient = inviteServiceClient;
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

        report.AuditLogs = await _auditLogServiceClient.ExecuteRequestAsync((c, ctx) => c.GetItemsCountOfGuildAsync(guildId, ctx.CancellationToken));
        report.PointTransactions = await _pointsServiceClient.ExecuteRequestAsync((c, ctx) => c.GetTransactionsCountForGuildAsync(guildId.ToString(), ctx.CancellationToken));
        report.UserMeasures = await _userMeasuresService.ExecuteRequestAsync((c, ctx) => c.GetItemsCountOfGuildAsync(guildId.ToString(), ctx.CancellationToken));
        report.EmoteStats = await _emoteService.ExecuteRequestAsync((c, ctx) => c.GetStatisticsCountInGuildAsync(guildId.ToString(), ctx.CancellationToken));
        report.Invites = await _inviteServiceClient.ExecuteRequestAsync((c, ctx) => c.GetInvitesCountOfGuildAsync(guildId, ctx.CancellationToken));

        return report;
    }
}
