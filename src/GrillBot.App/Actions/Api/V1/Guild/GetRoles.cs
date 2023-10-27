using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;

namespace GrillBot.App.Actions.Api.V1.Guild;

public class GetRoles : ApiAction
{
    private IDiscordClient DiscordClient { get; }

    public GetRoles(ApiRequestContext apiContext, IDiscordClient discordClient) : base(apiContext)
    {
        DiscordClient = discordClient;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var guildId = (ulong?)Parameters[0];

        var guilds = await GetGuildsAsync(guildId);

        var result = guilds
            .Select(o => o.Roles.Where(x => x.Id != o.EveryoneRole.Id))
            .SelectMany(o => o)
            .OrderBy(o => o.Name)
            .ToDictionary(o => o.Id.ToString(), o => o.Name);
        return ApiResult.Ok(result);
    }

    private async Task<List<IGuild>> GetGuildsAsync(ulong? guildId)
    {
        var guilds = ApiContext.IsPublic() ? await DiscordClient.FindMutualGuildsAsync(ApiContext.GetUserId()) : (await DiscordClient.GetGuildsAsync()).ToList();
        if (guildId != null)
            guilds = guilds.FindAll(o => o.Id == guildId.Value);
        return guilds;
    }
}
