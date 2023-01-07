using AutoMapper;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.Users;
using GrillBot.Database.Enums.Internal;

namespace GrillBot.App.Actions.Api.V1.Points;

public class ComputeUserPoints : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }
    private IMapper Mapper { get; }

    public ComputeUserPoints(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IDiscordClient discordClient, IMapper mapper) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;
        Mapper = mapper;
    }

    public async Task<List<UserPointsItem>> ProcessAsync(ulong? userId)
    {
        userId ??= ApiContext.GetUserId();
        var mutualGuilds = await GetGuildIdsAsync(userId.Value);

        await using var repository = DatabaseBuilder.CreateRepository();
        var points = await repository.Points.GetPointsBoardDataAsync(mutualGuilds, null, userId.Value, allColumns: true);
        return Mapper.Map<List<UserPointsItem>>(points);
    }

    private async Task<List<string>> GetGuildIdsAsync(ulong userId)
    {
        var ids = new List<string>();
        ids.AddRange((await DiscordClient.FindMutualGuildsAsync(userId)).ConvertAll(o => o.Id.ToString()));

        if (ApiContext.IsPublic())
            return ids;

        await using var repository = DatabaseBuilder.CreateRepository();

        var user = await repository.User.FindUserByIdAsync(userId, UserIncludeOptions.Guilds, true);
        if (user != null)
            ids.AddRange(user.Guilds.Select(o => o.GuildId));

        return ids.Distinct().ToList();
    }
}
