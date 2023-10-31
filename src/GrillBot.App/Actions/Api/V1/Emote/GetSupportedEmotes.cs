using AutoMapper;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Models.API.Emotes;

namespace GrillBot.App.Actions.Api.V1.Emote;

public class GetSupportedEmotes : ApiAction
{
    private IMapper Mapper { get; }
    private IDiscordClient DiscordClient { get; }

    public GetSupportedEmotes(ApiRequestContext apiContext, IMapper mapper, IDiscordClient discordClient) : base(apiContext)
    {
        Mapper = mapper;
        DiscordClient = discordClient;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var guilds = await DiscordClient.GetGuildsAsync();
        var result = new List<GuildEmoteItem>();

        foreach (var guild in guilds)
        {
            var emotes = guild.Emotes.ToList();
            var mappedEmotes = Mapper.Map<List<GuildEmoteItem>>(emotes);

            var mappedGuild = Mapper.Map<Data.Models.API.Guilds.Guild>(guild);
            foreach (var emote in mappedEmotes)
                emote.Guild = mappedGuild;

            result.AddRange(mappedEmotes);
        }

        result = result
            .OrderBy(o => o.Name)
            .ToList();
        return ApiResult.Ok(result);
    }
}
