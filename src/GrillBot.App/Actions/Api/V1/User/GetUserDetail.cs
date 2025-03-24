using AutoMapper;
using GrillBot.App.Managers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Services.PointsService;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Extensions;
using GrillBot.Database.Enums.Internal;
using GrillBot.Core.Infrastructure.Actions;
using ApiModels = GrillBot.Data.Models.API;
using Entity = GrillBot.Database.Entity;
using GrillBot.Core.Services.UserMeasures;
using GrillBot.Data.Enums;
using GrillBot.App.Managers.DataResolve;
using GrillBot.Core.Services.Emote;
using GrillBot.Core.Services.Emote.Models.Request;
using GrillBot.Data.Extensions.Services;
using GrillBot.Core.Services.UserMeasures.Models.Measures;
using GrillBot.Core.Services.Common.Executor;

namespace GrillBot.App.Actions.Api.V1.User;

public class GetUserDetail : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }
    private IDiscordClient DiscordClient { get; }
    private ITextsManager Texts { get; }
    private readonly DataResolveManager _dataResolveManager;
    private readonly IServiceClientExecutor<IEmoteServiceClient> _emoteServiceClient;
    private readonly IServiceClientExecutor<IPointsServiceClient> _pointsServiceClient;
    private readonly IServiceClientExecutor<IUserMeasuresServiceClient> _userMeasuresService;

    public GetUserDetail(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, IDiscordClient discordClient, ITextsManager texts,
        IServiceClientExecutor<IPointsServiceClient> pointsServiceClient, IServiceClientExecutor<IUserMeasuresServiceClient> userMeasuresService, DataResolveManager dataResolveManager,
        IServiceClientExecutor<IEmoteServiceClient> emoteServiceClient) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        DiscordClient = discordClient;
        Texts = texts;
        _pointsServiceClient = pointsServiceClient;
        _userMeasuresService = userMeasuresService;
        _dataResolveManager = dataResolveManager;
        _emoteServiceClient = emoteServiceClient;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var userId = (ulong?)Parameters.ElementAtOrDefault(0);
        userId ??= ApiContext.GetUserId();

        var result = await ProcessAsync(userId.Value);
        if (ApiContext.IsPublic())
            result.RemoveSecretData();
        return ApiResult.Ok(result);
    }

    public async Task<ApiModels.Users.UserDetail> ProcessAsync(ulong id)
    {
        using var repository = DatabaseBuilder.CreateRepository();

        var entity = await repository.User.FindUserByIdAsync(id, UserIncludeOptions.All, true)
            ?? throw new NotFoundException(Texts["User/NotFound", ApiContext.Language]);

        var result = new ApiModels.Users.UserDetail
        {
            Language = entity.Language,
            Id = entity.Id,
            Flags = entity.Flags,
            HaveBirthday = entity.Birthday is not null,
            Status = entity.Status,
            Username = entity.Username,
            SelfUnverifyMinimalTime = entity.SelfUnverifyMinimalTime,
            RegisteredAt = SnowflakeUtils.FromSnowflake(entity.Id.ToUlong()).LocalDateTime,
            AvatarUrl = entity.AvatarUrl,
            GlobalAlias = entity.GlobalAlias
        };

        await AddDiscordDataAsync(result);
        foreach (var guild in entity.Guilds)
            result.Guilds.Add(await CreateGuildDetailAsync(guild));

        result.Guilds = result.Guilds.OrderByDescending(o => o.IsUserInGuild).ThenBy(o => o.Guild.Name).ToList();
        return result;
    }

    private async Task AddDiscordDataAsync(ApiModels.Users.UserDetail result)
    {
        var user = await DiscordClient.FindUserAsync(result.Id.ToUlong());
        if (user is null) return;

        result.ActiveClients = user.ActiveClients.Select(o => o.ToString()).ToList();
        result.IsKnown = true;
    }

    private async Task<ApiModels.Users.GuildUserDetail> CreateGuildDetailAsync(Entity.GuildUser guildUserEntity)
    {
        var result = Mapper.Map<ApiModels.Users.GuildUserDetail>(guildUserEntity);

        result.CreatedInvites = result.CreatedInvites.OrderByDescending(o => o.CreatedAt).ToList();
        result.Channels = result.Channels.OrderByDescending(o => o.Count).ThenBy(o => o.Channel.Name).ToList();

        await SetUserMeasuresAsync(result, guildUserEntity);
        await SetPointsInfoAsync(result, guildUserEntity);
        await SetEmotesAsync(result, guildUserEntity);

        await UpdateGuildDetailAsync(result, guildUserEntity);
        return result;
    }

    private async Task UpdateGuildDetailAsync(ApiModels.Users.GuildUserDetail detail, Entity.GuildUser entity)
    {
        var guild = await DiscordClient.GetGuildAsync(detail.Guild.Id.ToUlong());
        if (guild is null) return;

        detail.IsGuildKnown = true;

        var guildUser = await guild.GetUserAsync(entity.UserId.ToUlong());
        if (guildUser is null) return;

        detail.IsUserInGuild = true;
        SetUnverify(detail, entity.Unverify, guildUser, guild);
        await SetVisibleChannelsAsync(detail, guildUser, guild);
        detail.Roles = Mapper.Map<List<ApiModels.Role>>(guildUser.GetRoles().OrderByDescending(o => o.Position).ToList());
    }

    private void SetUnverify(ApiModels.Users.GuildUserDetail detail, Entity.Unverify? unverify, IGuildUser user, IGuild guild)
    {
        if (unverify == null) return;

        var profile = UnverifyProfileManager.Reconstruct(unverify, user, guild);
        detail.Unverify = Mapper.Map<ApiModels.Unverify.UnverifyInfo>(profile);
    }

    private async Task SetVisibleChannelsAsync(ApiModels.Users.GuildUserDetail detail, IGuildUser user, IGuild guild)
    {
        if (ApiContext.IsPublic())
            return;

        var visibleChannels = await guild.GetAvailableChannelsAsync(user);

        detail.VisibleChannels = visibleChannels
            .Where(o => o is not ICategoryChannel)
            .Select(o => Mapper.Map<ApiModels.Channels.Channel>(o))
            .OrderBy(o => o.Name)
            .ToList();
    }

    private async Task SetUserMeasuresAsync(ApiModels.Users.GuildUserDetail detail, Entity.GuildUser entity)
    {
        var parameters = new MeasuresListParams
        {
            GuildId = entity.GuildId,
            UserId = entity.UserId,
            Pagination =
            {
                Page = 0,
                PageSize = int.MaxValue
            }
        };

        var measuresResult = await _userMeasuresService.ExecuteRequestAsync((c, cancellationToken) => c.GetMeasuresListAsync(parameters, cancellationToken));
        foreach (var measure in measuresResult.Data)
        {
            var moderator = await _dataResolveManager.GetUserAsync(measure.ModeratorId.ToUlong());

            detail.UserMeasures.Add(new ApiModels.UserMeasures.UserMeasuresItem
            {
                CreatedAt = measure.CreatedAtUtc.ToLocalTime(),
                Moderator = moderator!,
                ValidTo = measure.ValidTo?.ToLocalTime(),
                Type = measure.Type switch
                {
                    "Warning" => UserMeasuresType.Warning,
                    "Timeout" => UserMeasuresType.Timeout,
                    "Unverify" => UserMeasuresType.Unverify,
                    _ => 0
                },
                Reason = measure.Reason
            });
        }
    }

    private async Task SetPointsInfoAsync(ApiModels.Users.GuildUserDetail detail, Entity.GuildUser entity)
    {
        detail.HavePointsTransaction = await _pointsServiceClient.ExecuteRequestAsync((c, cancellationToken) => c.ExistsAnyTransactionAsync(entity.GuildId, entity.UserId, cancellationToken));
    }

    private async Task SetEmotesAsync(ApiModels.Users.GuildUserDetail detail, Entity.GuildUser entity)
    {
        var request = new EmoteStatisticsListRequest
        {
            GuildId = entity.GuildId,
            Pagination =
            {
                Page = 0,
                PageSize = int.MaxValue
            },
            Unsupported = false,
            UserId = entity.UserId
        };

        var response = await _emoteServiceClient.ExecuteRequestAsync((c, cancellationToken) => c.GetEmoteStatisticsListAsync(request, cancellationToken));
        detail.Emotes = response.Data.ConvertAll(o => new ApiModels.Emotes.EmoteStatItem
        {
            Emote = o.ToEmoteItem(),
            FirstOccurence = o.FirstOccurence,
            LastOccurence = o.LastOccurence,
            UseCount = o.UseCount
        });
    }
}
