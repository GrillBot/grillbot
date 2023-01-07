using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;

namespace GrillBot.App.Actions.Api.V1.User;

public class GetAvailableUsers : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public GetAvailableUsers(ApiRequestContext apiContext, IDiscordClient discordClient, GrillBotDatabaseBuilder databaseBuilder) : base(apiContext)
    {
        DiscordClient = discordClient;
        DatabaseBuilder = databaseBuilder;
    }

    public async Task<Dictionary<string, string>> ProcessAsync(bool? bots, ulong? guildId)
    {
        var mutualGuilds = await GetMutualGuildsAsync();

        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.User.GetFullListOfUsers(bots, mutualGuilds, guildId);
        return data.ToDictionary(o => o.Id, o => o.FullName());
    }

    private async Task<List<string>> GetMutualGuildsAsync()
    {
        if (!ApiContext.IsPublic()) return null;

        return (await DiscordClient.FindMutualGuildsAsync(ApiContext.GetUserId()))
            .ConvertAll(o => o.Id.ToString());
    }
}
