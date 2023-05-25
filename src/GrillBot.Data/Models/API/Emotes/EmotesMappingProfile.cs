using Discord;

namespace GrillBot.Data.Models.API.Emotes;

public class EmotesMappingProfile : AutoMapper.Profile
{
    public EmotesMappingProfile()
    {
        CreateMap<Database.Entity.EmoteStatisticItem, EmoteStatItem>()
            .ForMember(dst => dst.Emote, opt => opt.MapFrom(src => Emote.Parse(src.EmoteId)));

        CreateMap<Database.Models.Emotes.EmoteStatItem, EmoteStatItem>()
            .ForMember(dst => dst.Emote, opt => opt.MapFrom(src => Emote.Parse(src.EmoteId)));

        CreateMap<Database.Entity.EmoteStatisticItem, EmoteStatsUserListItem>()
            .ForMember(dst => dst.User, opt => opt.MapFrom(src => src.User!.User));
    }
}
