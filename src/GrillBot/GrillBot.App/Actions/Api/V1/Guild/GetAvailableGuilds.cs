using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.Guilds;

namespace GrillBot.App.Actions.Api.V1.Guild;

public class GetAvailableGuilds : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }

    public GetAvailableGuilds(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IDiscordClient discordClient) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;
    }

    public async Task<Dictionary<string, string>> ProcessAsync()
    {
        if (ApiContext.IsPublic())
            return await GetMutualGuildsAsync();

        var filter = new GetGuildListParams
        {
            Pagination = { Page = 1, PageSize = int.MaxValue }
        };

        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.Guild.GetGuildListAsync(filter, filter.Pagination);
        return data.Data.ToDictionary(o => o.Id, o => o.Name);
    }

    private async Task<Dictionary<string, string>> GetMutualGuildsAsync()
    {
        var loggedUserId = ApiContext.GetUserId();
        var mutualGuilds = await DiscordClient.FindMutualGuildsAsync(loggedUserId);
        return mutualGuilds.ToDictionary(o => o.Id.ToString(), o => o.Name);
    }
}
