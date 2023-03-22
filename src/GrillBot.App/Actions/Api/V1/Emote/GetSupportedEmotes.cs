using AutoMapper;
using GrillBot.Common.Models;
using GrillBot.Core.Managers.Discord;
using GrillBot.Data.Models.API.Emotes;

namespace GrillBot.App.Actions.Api.V1.Emote;

public class GetSupportedEmotes : ApiAction
{
    private IEmoteManager EmoteManager { get; }
    private IMapper Mapper { get; }

    public GetSupportedEmotes(ApiRequestContext apiContext, IMapper mapper, IEmoteManager emoteManager) : base(apiContext)
    {
        EmoteManager = emoteManager;
        Mapper = mapper;
    }

    public async Task<List<EmoteItem>> ProcessAsync()
    {
        var emotes = await EmoteManager.GetSupportedEmotesAsync();
        return Mapper.Map<List<EmoteItem>>(emotes).OrderBy(o => o.Name).ToList();
    }
}
