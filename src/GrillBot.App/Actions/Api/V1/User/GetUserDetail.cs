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
using GrillBot.Database.Services.Repository;
using ApiModels = GrillBot.Data.Models.API;
using Entity = GrillBot.Database.Entity;
using GrillBot.Core.Services.UserMeasures;
using GrillBot.Core.Services.UserMeasures.Models.MeasuresList;
using GrillBot.Data.Enums;

namespace GrillBot.App.Actions.Api.V1.User;

public class GetUserDetail : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }
    private IDiscordClient DiscordClient { get; }
    private ITextsManager Texts { get; }
    private IPointsServiceClient PointsServiceClient { get; }
    private IUserMeasuresServiceClient UserMeasuresService { get; }

    public GetUserDetail(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, IDiscordClient discordClient, ITextsManager texts,
        IPointsServiceClient pointsServiceClient, IUserMeasuresServiceClient userMeasuresService) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        DiscordClient = discordClient;
        Texts = texts;
        PointsServiceClient = pointsServiceClient;
        UserMeasuresService = userMeasuresService;
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
        await using var repository = DatabaseBuilder.CreateRepository();

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
            result.Guilds.Add(await CreateGuildDetailAsync(repository, guild));

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

    private async Task<ApiModels.Users.GuildUserDetail> CreateGuildDetailAsync(GrillBotRepository repository, Entity.GuildUser guildUserEntity)
    {
        var result = Mapper.Map<ApiModels.Users.GuildUserDetail>(guildUserEntity);

        result.CreatedInvites = result.CreatedInvites.OrderByDescending(o => o.CreatedAt).ToList();
        result.Channels = result.Channels.OrderByDescending(o => o.Count).ThenBy(o => o.Channel.Name).ToList();
        result.Emotes = result.Emotes.OrderByDescending(o => o.UseCount).ThenByDescending(o => o.LastOccurence).ThenBy(o => o.Emote.Name).ToList();

        await UpdateGuildDetailAsync(repository, result, guildUserEntity);
        return result;
    }

    private async Task UpdateGuildDetailAsync(GrillBotRepository repository, ApiModels.Users.GuildUserDetail detail, Entity.GuildUser entity)
    {
        await SetUserMeasuresAsync(repository, detail, entity);
        detail.HavePointsTransaction = await PointsServiceClient.ExistsAnyTransactionAsync(entity.GuildId, entity.UserId);

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
            .Select(o => Mapper.Map<Data.Models.API.Channels.Channel>(o))
            .OrderBy(o => o.Name)
            .ToList();
    }

    private async Task SetUserMeasuresAsync(GrillBotRepository repository, ApiModels.Users.GuildUserDetail detail, Entity.GuildUser entity)
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

        var measuresResult = await UserMeasuresService.GetMeasuresListAsync(parameters);
        measuresResult.ValidationErrors.AggregateAndThrow();

        var measures = measuresResult.Response!.Data;

        var moderatorIds = measures.Select(o => o.ModeratorId).Distinct().ToList();
        var moderators = await repository.User.GetUsersByIdsAsync(moderatorIds);

        foreach (var measure in measures)
        {
            var moderator = moderators.Find(o => o.Id == measure.ModeratorId);

            detail.UserMeasures.Add(new ApiModels.UserMeasures.UserMeasuresItem
            {
                CreatedAt = measure.CreatedAtUtc.ToLocalTime(),
                Moderator = Mapper.Map<ApiModels.Users.User>(moderator),
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
}
