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
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.AuditLog.Models.Response.Search;
using GrillBot.Core.Services.AuditLog.Models.Request.Search;
using GrillBot.Core.Services.AuditLog.Enums;
using System.Text.Json;
using GrillBot.Database.Services.Repository;
using ApiModels = GrillBot.Data.Models.API;
using Entity = GrillBot.Database.Entity;

namespace GrillBot.App.Actions.Api.V1.User;

public class GetUserDetail : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }
    private IDiscordClient DiscordClient { get; }
    private ITextsManager Texts { get; }
    private IPointsServiceClient PointsServiceClient { get; }
    private IAuditLogServiceClient AuditLogServiceClient { get; }

    public GetUserDetail(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, IDiscordClient discordClient, ITextsManager texts,
        IPointsServiceClient pointsServiceClient, IAuditLogServiceClient auditLogServiceClient) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        DiscordClient = discordClient;
        Texts = texts;
        PointsServiceClient = pointsServiceClient;
        AuditLogServiceClient = auditLogServiceClient;
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
        var memberWarnings = await ReadMemberWarningsAsync(entity);
        var unverifyLogs = await ReadUnverifyLogsAsync(repository, entity);

        var moderatorIds = memberWarnings
            .Select(o => o.UserId!)
            .Concat(unverifyLogs.Select(o => o.FromUserId!))
            .Where(o => !string.IsNullOrEmpty(o))
            .Distinct()
            .ToList();
        var moderators = await repository.User.GetUsersByIdsAsync(moderatorIds);

        foreach (var item in memberWarnings)
        {
            var moderator = moderators.Find(o => o.Id == item.UserId);
            var preview = (MemberWarningPreview)item.Preview!;

            detail.UserMeasures.Add(new ApiModels.Users.UserMeasuresItem
            {
                CreatedAt = item.CreatedAt.ToLocalTime(),
                Moderator = Mapper.Map<ApiModels.Users.User>(moderator),
                Reason = preview.Reason,
                Type = Data.Enums.UserMeasuresType.Warning,
            });
        }

        foreach (var item in unverifyLogs)
        {
            var moderator = moderators.Find(o => o.Id == item.FromUserId);
            var logData = JsonConvert.DeserializeObject<Data.Models.Unverify.UnverifyLogSet>(item.Data)!;

            detail.UserMeasures.Add(new ApiModels.Users.UserMeasuresItem
            {
                CreatedAt = logData.Start,
                Moderator = Mapper.Map<ApiModels.Users.User>(moderator),
                Reason = logData.Reason!,
                Type = Data.Enums.UserMeasuresType.Unverify,
                ValidTo = logData.End
            });
        }

        detail.UserMeasures = detail.UserMeasures.OrderByDescending(o => o.CreatedAt).ToList();
    }

    private async Task<List<LogListItem>> ReadMemberWarningsAsync(Entity.GuildUser entity)
    {
        var searchRequest = new SearchRequest
        {
            GuildId = entity.GuildId,
            Pagination =
            {
                Page = 0,
                PageSize = int.MaxValue
            },
            ShowTypes = new List<LogType> { LogType.MemberWarning },
            AdvancedSearch = new AdvancedSearchRequest
            {
                MemberWarning = new UserIdSearchRequest
                {
                    UserId = entity.UserId
                }
            }
        };

        var searchResult = await AuditLogServiceClient.SearchItemsAsync(searchRequest);
        searchResult.ValidationErrors.AggregateAndThrow();

        var serializationOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        };

        foreach (var item in searchResult.Response!.Data.Where(o => o.Preview is JsonElement))
            item.Preview = ((JsonElement)item.Preview!).Deserialize<MemberWarningPreview>(serializationOptions);

        return searchResult.Response!.Data;
    }

    private static async Task<List<Entity.UnverifyLog>> ReadUnverifyLogsAsync(GrillBotRepository repository, Entity.GuildUser entity)
    {
        var logRequest = new ApiModels.Unverify.UnverifyLogParams
        {
            GuildId = entity.GuildId,
            Operation = Database.Enums.UnverifyOperation.Unverify,
            Pagination =
            {
                Page = 0,
                PageSize = int.MaxValue
            },
            ToUserId = entity.UserId
        };

        var result = await repository.Unverify.GetLogsAsync(logRequest, logRequest.Pagination, new List<string>());
        return result.Data;
    }
}
