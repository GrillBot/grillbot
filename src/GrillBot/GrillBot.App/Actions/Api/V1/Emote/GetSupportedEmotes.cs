using AutoMapper;
using GrillBot.App.Helpers;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.Emotes;

namespace GrillBot.App.Actions.Api.V1.Emote;

public class GetSupportedEmotes : ApiAction
{
    private EmoteHelper EmoteHelper { get; }
    private IMapper Mapper { get; }

    public GetSupportedEmotes(ApiRequestContext apiContext, IMapper mapper, EmoteHelper emoteHelper) : base(apiContext)
    {
        EmoteHelper = emoteHelper;
        Mapper = mapper;
    }

    public async Task<List<EmoteItem>> ProcessAsync()
    {
        var emotes = await EmoteHelper.GetSupportedEmotesAsync();
        return Mapper.Map<List<EmoteItem>>(emotes).OrderBy(o => o.Name).ToList();
    }
}
