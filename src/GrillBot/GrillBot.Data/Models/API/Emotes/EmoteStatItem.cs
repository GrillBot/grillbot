using Discord;
using System;

namespace GrillBot.Data.Models.API.Emotes;

public class EmoteStatItem
{
    public EmoteItem Emote { get; set; }
    public long UseCount { get; set; }
    public DateTime FirstOccurence { get; set; }
    public DateTime LastOccurence { get; set; }
    public int UsedUsersCount { get; set; }
}

public class EmoteStatItemMappingProfile : AutoMapper.Profile
{
    public EmoteStatItemMappingProfile()
    {
        CreateMap<Database.Entity.EmoteStatisticItem, EmoteStatItem>()
            .ForMember(dst => dst.Emote, opt => opt.MapFrom(src => Emote.Parse(src.EmoteId)));

        CreateMap<Models.EmoteStatItem, EmoteStatItem>()
            .ForMember(dst => dst.Emote, opt => opt.MapFrom(src => Emote.Parse(src.Id)))
            .ForMember(dst => dst.UsedUsersCount, opt => opt.MapFrom(src => src.UsersCount));
    }
}
