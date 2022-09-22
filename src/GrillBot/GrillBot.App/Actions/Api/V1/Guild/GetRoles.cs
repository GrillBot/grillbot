using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;

namespace GrillBot.App.Actions.Api.V1.Guild;

public class GetRoles : ApiAction
{
    private IDiscordClient DiscordClient { get; }

    public GetRoles(ApiRequestContext apiContext, IDiscordClient discordClient) : base(apiContext)
    {
        DiscordClient = discordClient;
    }

    public async Task<Dictionary<string, string>> ProcessAsync(ulong? guildId)
    {
        var guilds = await GetGuildsAsync(guildId);

        return guilds
            .Select(o => o.Roles.Where(x => x.Id != o.EveryoneRole.Id))
            .SelectMany(o => o)
            .OrderBy(o => o.Name)
            .ToDictionary(o => o.Id.ToString(), o => o.Name);
    }

    private async Task<List<IGuild>> GetGuildsAsync(ulong? guildId)
    {
        var guilds = ApiContext.IsPublic() ? await DiscordClient.FindMutualGuildsAsync(ApiContext.GetUserId()) : (await DiscordClient.GetGuildsAsync()).ToList();
        if (guildId != null)
            guilds = guilds.FindAll(o => o.Id == guildId.Value);
        return guilds;
    }
}
