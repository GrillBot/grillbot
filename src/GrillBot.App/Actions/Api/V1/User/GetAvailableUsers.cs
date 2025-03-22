using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;

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

    public override async Task<ApiResult> ProcessAsync()
    {
        var bots = (bool?)Parameters[0]!;
        var guildId = (ulong?)Parameters[1]!;
        var mutualGuilds = await GetMutualGuildsAsync();

        using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.User.GetFullListOfUsers(bots, mutualGuilds, guildId);
        var result = data
            .Select(o => new { o.Id, DisplayName = o.GetDisplayName() })
            .OrderBy(o => o.DisplayName)
            .ToDictionary(o => o.Id, o => o.DisplayName);
        return ApiResult.Ok(result);
    }

    private async Task<List<string>?> GetMutualGuildsAsync()
    {
        if (!ApiContext.IsPublic())
            return null;

        return (await DiscordClient.FindMutualGuildsAsync(ApiContext.GetUserId()))
            .ConvertAll(o => o.Id.ToString());
    }
}
