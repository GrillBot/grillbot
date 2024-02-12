using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Models.API.UserMeasures;
using GrillBot.Core.Extensions;
using GrillBot.Database.Services.Repository;
using GrillBot.Core.Models.Pagination;
using ApiModels = GrillBot.Data.Models.API;
using AutoMapper;
using GrillBot.Core.Services.UserMeasures;
using GrillBot.Core.Services.UserMeasures.Models.MeasuresList;
using GrillBot.Data.Enums;

namespace GrillBot.App.Actions.Api.V1.UserMeasures;

public class GetUserMeasuresList : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }
    private IUserMeasuresServiceClient UserMeasuresService { get; }

    private Dictionary<string, ApiModels.Users.User> CachedUsers { get; } = new();
    private Dictionary<string, ApiModels.Guilds.Guild> CachedGuilds { get; } = new();

    public GetUserMeasuresList(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, IUserMeasuresServiceClient userMeasuresService) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        UserMeasuresService = userMeasuresService;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = (MeasuresListParams)Parameters[0]!;

        if (parameters.CreatedFrom.HasValue)
            parameters.CreatedFrom = parameters.CreatedFrom.Value.WithKind(DateTimeKind.Local).ToUniversalTime();
        if (parameters.CreatedTo.HasValue)
            parameters.CreatedTo = parameters.CreatedTo.Value.WithKind(DateTimeKind.Local).ToUniversalTime();

        var measures = await UserMeasuresService.GetMeasuresListAsync(parameters);
        measures.ValidationErrors.AggregateAndThrow();

        var result = await MapItemsAsync(measures.Response!);
        return ApiResult.Ok(result);
    }

    private async Task<PaginatedResponse<UserMeasuresListItem>> MapItemsAsync(PaginatedResponse<MeasuresItem> response)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        return await PaginatedResponse<UserMeasuresListItem>.CopyAndMapAsync(response, async entity => new UserMeasuresListItem
        {
            CreatedAt = entity.CreatedAtUtc.ToLocalTime(),
            Guild = await ReadGuildAsync(repository, entity.GuildId),
            Moderator = await ReadUserAsync(repository, entity.ModeratorId),
            Reason = entity.Reason,
            Type = entity.Type switch
            {
                "Warning" => UserMeasuresType.Warning,
                "Timeout" => UserMeasuresType.Timeout,
                "Unverify" => UserMeasuresType.Unverify,
                _ => 0
            },
            User = await ReadUserAsync(repository, entity.UserId),
            ValidTo = entity.ValidTo
        });
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
}
