using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
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

    public override async Task<ApiResult> ProcessAsync()
    {
        if (ApiContext.IsPublic())
            return ApiResult.Ok(await GetMutualGuildsAsync());

        var filter = new GetGuildListParams
        {
            Pagination = { Page = 0, PageSize = int.MaxValue }
        };

        using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.Guild.GetGuildListAsync(filter, filter.Pagination);
        return ApiResult.Ok(data.Data.ToDictionary(o => o.Id, o => o.Name));
    }

    private async Task<Dictionary<string, string>> GetMutualGuildsAsync()
    {
        var loggedUserId = ApiContext.GetUserId();
        var mutualGuilds = await DiscordClient.FindMutualGuildsAsync(loggedUserId);
        return mutualGuilds.ToDictionary(o => o.Id.ToString(), o => o.Name);
    }
}
