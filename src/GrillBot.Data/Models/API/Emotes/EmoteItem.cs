using Discord;

namespace GrillBot.Data.Models.API.Emotes;

public class EmoteItem
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string ImageUrl { get; set; }
    public string FullId { get; set; }
}

public class EmoteItemMappingProfile : AutoMapper.Profile
{
    public EmoteItemMappingProfile()
    {
        CreateMap<Emote, EmoteItem>()
            .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dst => dst.ImageUrl, opt => opt.MapFrom(src => src.Url))
            .ForMember(dst => dst.FullId, opt => opt.MapFrom(src => src.ToString()));
    }
}
