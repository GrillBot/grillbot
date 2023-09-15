using AutoMapper;
using GrillBot.Common.Models;
using GrillBot.Core.Extensions;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.PointsService;
using GrillBot.Core.Services.PointsService.Models.Users;
using GrillBot.Database.Enums.Internal;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Actions.Api.V1.Points;

public class GetUserList : ApiAction
{
    private IPointsServiceClient PointsServiceClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }

    private Dictionary<string, Data.Models.API.Guilds.Guild> CachedGuilds { get; } = new();
    private Dictionary<string, Data.Models.API.Users.User> CachedUsers { get; } = new();

    public GetUserList(ApiRequestContext apiContext, IPointsServiceClient pointsServiceClient, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper) : base(apiContext)
    {
        PointsServiceClient = pointsServiceClient;
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
    }

    public async Task<PaginatedResponse<Data.Models.API.Points.UserListItem>> ProcessAsync(UserListRequest request)
    {
        var userList = await PointsServiceClient.GetUserListAsync(request);
        await using var repository = DatabaseBuilder.CreateRepository();

        var userIds = userList.Data.Select(o => o.UserId).Distinct().ToList();
        var users = await repository.User.GetUsersByIdsAsync(userIds);
        foreach (var user in users)
            CachedUsers.Add(user.Id, Mapper.Map<Data.Models.API.Users.User>(user));

        var guildIds = userList.Data.Select(o => o.GuildId).Distinct().ToList();
        var guilds = await repository.Guild.GetGuildsByIdsAsync(guildIds);
        foreach (var guild in guilds)
            CachedGuilds.Add(guild.Id, Mapper.Map<Data.Models.API.Guilds.Guild>(guild));

        return await PaginatedResponse<Data.Models.API.Points.UserListItem>.CopyAndMapAsync(userList, entity => MapItemAsync(entity, repository));
    }

    private async Task<Data.Models.API.Points.UserListItem> MapItemAsync(UserListItem item, GrillBotRepository repository)
    {
        return new Data.Models.API.Points.UserListItem
        {
            ActivePoints = item.ActivePoints,
            ExpiredPoints = item.ExpiredPoints,
            Guild = await GetGuildAsync(repository, item.GuildId),
            User = await GetUserAsync(repository, item.UserId),
            MergedPoints = item.MergedPoints,
            PointsDeactivated = item.PointsDeactivated
        };
    }

    private async Task<Data.Models.API.Guilds.Guild> GetGuildAsync(GrillBotRepository repository, string guildId)
    {
        if (CachedGuilds.TryGetValue(guildId, out var guild))
            return guild;

        var guildEntity = await repository.Guild.FindGuildByIdAsync(guildId.ToUlong(), true);
        guild = Mapper.Map<Data.Models.API.Guilds.Guild>(guildEntity);

        CachedGuilds.Add(guildId, guild);
        return guild;
    }

    private async Task<Data.Models.API.Users.User> GetUserAsync(GrillBotRepository repository, string userId)
    {
        if (CachedUsers.TryGetValue(userId, out var user))
            return user;

        var userEntity = await repository.User.FindUserByIdAsync(userId.ToUlong(), UserIncludeOptions.None, true);
        userEntity ??= new Database.Entity.User
        {
            Id = userId,
            Username = userId
        };

        user = Mapper.Map<Data.Models.API.Users.User>(userEntity);
        CachedUsers.Add(userId, user);
        return user;
    }
}
