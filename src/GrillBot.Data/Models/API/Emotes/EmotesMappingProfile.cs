using Discord;

namespace GrillBot.Data.Models.API.Emotes;

public class EmotesMappingProfile : AutoMapper.Profile
{
    public EmotesMappingProfile()
    {
        CreateMap<Database.Models.Emotes.EmoteStatItem, EmoteStatItem>()
            .ForMember(dst => dst.Emote, opt => opt.MapFrom(src => Emote.Parse(src.EmoteId)));

        CreateMap<Emote, EmoteItem>()
            .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dst => dst.ImageUrl, opt => opt.MapFrom(src => src.Url))
            .ForMember(dst => dst.FullId, opt => opt.MapFrom(src => src.ToString()));
    }
}
