using AutoMapper;
using GrillBot.App.Services.Emotes;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.Emotes;

namespace GrillBot.App.Actions.Api.V1.Emote;

public class GetSupportedEmotes : ApiAction
{
    private EmotesCacheService Cache { get; }
    private IMapper Mapper { get; }

    public GetSupportedEmotes(ApiRequestContext apiContext, EmotesCacheService cache, IMapper mapper) : base(apiContext)
    {
        Cache = cache;
        Mapper = mapper;
    }

    public List<EmoteItem> Process()
    {
        var emotes = Cache.GetSupportedEmotes();
        return Mapper.Map<List<EmoteItem>>(emotes).OrderBy(o => o.Name).ToList();
    }
}
