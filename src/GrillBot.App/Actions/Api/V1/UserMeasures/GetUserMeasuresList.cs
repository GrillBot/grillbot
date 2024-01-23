using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Models.API.UserMeasures;
using AuditLogService = GrillBot.Core.Services.AuditLog;
using AuditLogModels = GrillBot.Core.Services.AuditLog.Models;
using GrillBot.Core.Extensions;
using GrillBot.Database.Entity;
using GrillBot.Database.Services.Repository;
using GrillBot.Data.Models.API.Unverify;
using GrillBot.Core.Models.Pagination;
using ApiModels = GrillBot.Data.Models.API;
using AutoMapper;
using System.Text.Json;
using GrillBot.Core.Services.AuditLog.Models.Response.Search;

namespace GrillBot.App.Actions.Api.V1.UserMeasures;

public class GetUserMeasuresList : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private AuditLogService.IAuditLogServiceClient AuditLogServiceClient { get; }
    private IMapper Mapper { get; }

    private Dictionary<string, ApiModels.Users.User> CachedUsers { get; } = new();
    private Dictionary<string, ApiModels.Guilds.Guild> CachedGuilds { get; } = new();

    public GetUserMeasuresList(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper,
        AuditLogService.IAuditLogServiceClient auditLogServiceClient) : base(apiContext)
    {
        AuditLogServiceClient = auditLogServiceClient;
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var parameters = (UserMeasuresParams)Parameters[0]!;
        var memberWarnings = await ReadMemberWarningsAsync(parameters);
        var unverifyLogs = await ReadUnverifyLogsAsync(repository, parameters);
        var allItems = await MergeAndMapAsync(repository, memberWarnings, unverifyLogs);
        var result = PaginatedResponse<UserMeasuresListItem>.Create(FilterItems(allItems, parameters), parameters.Pagination);

        return ApiResult.Ok(result);
    }

    private async Task<List<LogListItem>> ReadMemberWarningsAsync(UserMeasuresParams parameters)
    {
        var createdFrom = parameters.CreatedFrom is not null ? parameters.CreatedFrom.Value.WithKind(DateTimeKind.Local).ToUniversalTime() : (DateTime?)null;
        var createdto = parameters.CreatedTo is not null ? parameters.CreatedTo.Value.WithKind(DateTimeKind.Local).ToUniversalTime() : (DateTime?)null;

        var searchRequest = new AuditLogModels.Request.Search.SearchRequest
        {
            CreatedFrom = createdFrom,
            CreatedTo = createdto,
            GuildId = parameters.GuildId,
            Pagination =
            {
                Page = 0,
                PageSize = int.MaxValue
            },
            ShowTypes = new List<AuditLogService.Enums.LogType> { AuditLogService.Enums.LogType.MemberWarning },
            Sort =
            {
                Descending = true,
                OrderBy = "CreatedAt"
            }
        };

        if (!string.IsNullOrEmpty(parameters.ModeratorId))
            searchRequest.UserIds = new List<string> { parameters.ModeratorId! };

        if (!string.IsNullOrEmpty(parameters.UserId))
        {
            searchRequest.AdvancedSearch = new AuditLogModels.Request.Search.AdvancedSearchRequest
            {
                MemberWarning = new AuditLogModels.Request.Search.UserIdSearchRequest
                {
                    UserId = parameters.UserId
                }
            };
        }

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

    private static async Task<List<UnverifyLog>> ReadUnverifyLogsAsync(GrillBotRepository repository, UserMeasuresParams parameters)
    {
        var logParams = new UnverifyLogParams
        {
            FromUserId = parameters.ModeratorId,
            GuildId = parameters.GuildId,
            Operation = Database.Enums.UnverifyOperation.Unverify,
            Pagination =
            {
                Page = 0,
                PageSize = int.MaxValue
            },
            Sort =
            {
                Descending = true,
                OrderBy = "CreatedAt"
            },
            ToUserId = parameters.UserId,
            Created = new Database.Models.RangeParams<DateTime?>
            {
                From = parameters.CreatedFrom,
                To = parameters.CreatedTo
            }
        };

        var result = await repository.Unverify.GetLogsAsync(logParams, logParams.Pagination, new List<string>());
        return result.Data;
    }

    private async Task<List<UserMeasuresListItem>> MergeAndMapAsync(GrillBotRepository repository, List<LogListItem> memberWarnings, List<UnverifyLog> unverifyLogs)
    {
        var result = new List<UserMeasuresListItem>();

        foreach (var warning in memberWarnings)
        {
            var preview = (MemberWarningPreview)warning.Preview!;

            result.Add(new UserMeasuresListItem
            {
                CreatedAt = warning.CreatedAt.ToLocalTime(),
                Guild = await ReadGuildAsync(repository, warning.GuildId!),
                Moderator = await ReadUserAsync(repository, warning.UserId!),
                Reason = preview.Reason,
                Type = Data.Enums.UserMeasuresType.Warning,
                User = await ReadUserAsync(repository, preview.TargetId)
            });
        }

        foreach (var item in unverifyLogs)
        {
            var logData = JsonConvert.DeserializeObject<Data.Models.Unverify.UnverifyLogSet>(item.Data)!;

            result.Add(new UserMeasuresListItem
            {
                CreatedAt = logData.Start,
                Guild = await ReadGuildAsync(repository, item.GuildId),
                Moderator = await ReadUserAsync(repository, item.FromUserId),
                Reason = logData.Reason!,
                Type = Data.Enums.UserMeasuresType.Unverify,
                User = await ReadUserAsync(repository, item.ToUserId),
                ValidTo = logData.End
            });
        }

        return result.OrderByDescending(o => o.CreatedAt).ToList();
    }

    private async Task<ApiModels.Guilds.Guild> ReadGuildAsync(GrillBotRepository repository, string guildId)
    {
        if (CachedGuilds.TryGetValue(guildId, out var guild))
            return guild;

        var entity = await repository.Guild.FindGuildByIdAsync(guildId.ToUlong(), true);
        guild = Mapper.Map<ApiModels.Guilds.Guild>(entity);
        CachedGuilds.Add(guildId, guild);

        return guild;
    }

    private async Task<ApiModels.Users.User> ReadUserAsync(GrillBotRepository repository, string userId)
    {
        if (CachedUsers.TryGetValue(userId, out var user))
            return user;

        var entity = await repository.User.FindUserByIdAsync(userId.ToUlong(), disableTracking: true);
        user = Mapper.Map<ApiModels.Users.User>(entity);
        CachedUsers.Add(userId, user);

        return user;
    }

    private static List<UserMeasuresListItem> FilterItems(List<UserMeasuresListItem> items, UserMeasuresParams parameters)
    {
        return parameters.Type is not null ?
            items.FindAll(o => o.Type == parameters.Type.Value) :
            items;
    }
}
