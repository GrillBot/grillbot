using AutoMapper;
using GrillBot.App.Managers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Common.Services.PointsService;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Extensions;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.AuditLog.Filters;
using GrillBot.Data.Models.API.Unverify;
using GrillBot.Data.Models.API.Users;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using GrillBot.Database.Enums.Internal;
using GrillBot.Database.Models;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Actions.Api.V1.User;

public class GetUserDetail : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }
    private IDiscordClient DiscordClient { get; }
    private ITextsManager Texts { get; }
    private IPointsServiceClient PointsServiceClient { get; }

    public GetUserDetail(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, IDiscordClient discordClient, ITextsManager texts,
        IPointsServiceClient pointsServiceClient) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        DiscordClient = discordClient;
        Texts = texts;
        PointsServiceClient = pointsServiceClient;
    }

    public async Task<UserDetail> ProcessSelfAsync()
    {
        var userId = ApiContext.GetUserId();

        var result = await ProcessAsync(userId);
        if (ApiContext.IsPublic())
            result.RemoveSecretData();
        return result;
    }

    public async Task<UserDetail> ProcessAsync(ulong id)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var entity = await repository.User.FindUserByIdAsync(id, UserIncludeOptions.All, true);
        if (entity == null)
            throw new NotFoundException(Texts["User/NotFound", ApiContext.Language]);

        var result = new UserDetail
        {
            Language = entity.Language,
            Id = entity.Id,
            Discriminator = entity.Discriminator,
            Flags = entity.Flags,
            Note = entity.Note,
            HaveBirthday = entity.Birthday != null,
            Status = entity.Status,
            Username = entity.Username,
            SelfUnverifyMinimalTime = entity.SelfUnverifyMinimalTime,
            RegisteredAt = SnowflakeUtils.FromSnowflake(entity.Id.ToUlong()).LocalDateTime
        };

        await AddDiscordDataAsync(result);
        foreach (var guild in entity.Guilds)
            result.Guilds.Add(await CreateGuildDetailAsync(repository, guild));

        result.Guilds = result.Guilds.OrderByDescending(o => o.IsUserInGuild).ThenBy(o => o.Guild.Name).ToList();
        return result;
    }

    private async Task AddDiscordDataAsync(UserDetail result)
    {
        var user = await DiscordClient.FindUserAsync(result.Id.ToUlong());
        if (user == null) return;

        result.ActiveClients = user.ActiveClients.Select(o => o.ToString()).ToList();
        result.AvatarUrl = user.GetUserAvatarUrl();
        result.IsKnown = true;
    }

    private async Task<GuildUserDetail> CreateGuildDetailAsync(GrillBotRepository repository, Database.Entity.GuildUser guildUserEntity)
    {
        var result = Mapper.Map<GuildUserDetail>(guildUserEntity);

        result.CreatedInvites = result.CreatedInvites.OrderByDescending(o => o.CreatedAt).ToList();
        result.Channels = result.Channels.OrderByDescending(o => o.Count).ThenBy(o => o.Channel.Name).ToList();
        result.Emotes = result.Emotes.OrderByDescending(o => o.UseCount).ThenByDescending(o => o.LastOccurence).ThenBy(o => o.Emote.Name).ToList();

        await UpdateGuildDetailAsync(repository, result, guildUserEntity);
        return result;
    }

    private async Task UpdateGuildDetailAsync(GrillBotRepository repository, GuildUserDetail detail, Database.Entity.GuildUser entity)
    {
        var guild = await DiscordClient.GetGuildAsync(detail.Guild.Id.ToUlong());
        if (guild == null) return;

        detail.IsGuildKnown = true;

        var guildUser = await guild.GetUserAsync(entity.UserId.ToUlong());
        if (guildUser == null) return;

        detail.IsUserInGuild = true;
        SetUnverify(detail, entity.Unverify, guildUser, guild);
        await SetVisibleChannelsAsync(detail, guildUser, guild);
        detail.Roles = Mapper.Map<List<Role>>(guildUser.GetRoles().OrderByDescending(o => o.Position).ToList());
        detail.HavePointsTransaction = await PointsServiceClient.ExistsAnyTransactionAsync(guildUser.GuildId.ToString(), guildUser.Id.ToString());
        await SetTimeoutHistoryAsync(repository, detail, guildUser);
    }

    private void SetUnverify(GuildUserDetail detail, Database.Entity.Unverify? unverify, IGuildUser user, IGuild guild)
    {
        if (unverify == null) return;

        var profile = UnverifyProfileManager.Reconstruct(unverify, user, guild);
        detail.Unverify = Mapper.Map<UnverifyInfo>(profile);
    }

    private async Task SetVisibleChannelsAsync(GuildUserDetail detail, IGuildUser user, IGuild guild)
    {
        var visibleChannels = await guild.GetAvailableChannelsAsync(user);

        detail.VisibleChannels = visibleChannels
            .Where(o => o is not ICategoryChannel)
            .Select(o => Mapper.Map<Data.Models.API.Channels.Channel>(o))
            .OrderBy(o => o.Name)
            .ToList();
    }

    private async Task SetTimeoutHistoryAsync(GrillBotRepository repository, GuildUserDetail detail, IGuildUser user)
    {
        if (ApiContext.IsPublic())
            return;

        var parameters = new AuditLogListParams
        {
            Sort = new SortParams { Descending = false, OrderBy = "CreatedAt" },
            GuildId = user.GuildId.ToString(),
            Types = new List<AuditLogItemType> { AuditLogItemType.MemberUpdated }
        };

        var data = await repository.AuditLog.GetSimpleDataAsync(parameters);

        var query = data
            .Select(o => new { Data = JsonConvert.DeserializeObject<MemberUpdatedData>(o.Data, AuditLogWriteManager.SerializerSettings)!, o.CreatedAt })
            .Where(o => o.Data.Target.UserId == user.Id.ToString() && o.Data.TimeoutUntil is not null);

        foreach (var item in query)
        {
            detail.TimeoutHistory.Add(new RangeParams<DateTime>
            {
                From = item.Data.TimeoutUntil!.Before ?? item.CreatedAt,
                To = item.Data.TimeoutUntil!.After ?? item.CreatedAt
            });
        }
    }
}
