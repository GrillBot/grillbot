using AutoMapper;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Data.Models.API.Points;
using GrillBot.Data.Models.API.Users;
using GrillBot.Database.Enums.Internal;

namespace GrillBot.App.Services.User.Points;

public class PointsApiService
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }
    private IDiscordClient DiscordClient { get; }

    public PointsApiService(GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, IDiscordClient discordClient)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        DiscordClient = discordClient;
    }

    public async Task<List<PointsSummaryBase>> GetGraphDataAsync(GetPointsSummaryParams parameters)
    {
        parameters.Sort.OrderBy = "Day";
        parameters.Sort.Descending = false;

        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.Points.GetGraphDataAsync(parameters);
        return data.ConvertAll(o => Mapper.Map<PointsSummaryBase>(o));
    }

    public async Task<List<UserPointsItem>> ComputeUserPointsAsync(ulong userId, bool onlyMutualGuilds)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        List<string> guildIds = new();
        if (onlyMutualGuilds)
        {
            guildIds = (await DiscordClient.FindMutualGuildsAsync(userId)).ConvertAll(o => o.Id.ToString());
        }
        else
        {
            var user = await repository.User.FindUserByIdAsync(userId, UserIncludeOptions.Guilds, true);
            if (user != null)
                guildIds = user.Guilds.Select(o => o.GuildId).ToList();
        }

        var pointsData = await repository.Points.GetPointsBoardDataAsync(guildIds, userId: userId);
        return Mapper.Map<List<UserPointsItem>>(pointsData);
    }
}
