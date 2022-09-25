using AutoMapper;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.Users;

namespace GrillBot.App.Actions.Api.V1.Points;

public class GetPointsLeaderboard : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }

    public GetPointsLeaderboard(ApiRequestContext apiContext, IDiscordClient discordClient, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper) : base(apiContext)
    {
        DiscordClient = discordClient;
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
    }

    public async Task<List<UserPointsItem>> ProcessAsync()
    {
        var mutualGuilds = await DiscordClient.FindMutualGuildsAsync(ApiContext.GetUserId());

        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.Points.GetPointsBoardDataAsync(mutualGuilds.ConvertAll(o => o.Id.ToString()));
        return Mapper.Map<List<UserPointsItem>>(data);
    }
}
