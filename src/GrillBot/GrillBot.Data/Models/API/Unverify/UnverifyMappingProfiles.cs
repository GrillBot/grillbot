using System;

namespace GrillBot.Data.Models.API.Unverify;

public class UnverifyMappingProfiles : AutoMapper.Profile
{
    public UnverifyMappingProfiles()
    {
        CreateMap<Database.Entity.UnverifyLog, UnverifyLogItem>();

        CreateMap<Models.Unverify.UnverifyUserProfile, UnverifyInfo>()
            .ForMember(dst => dst.EndTo, opt => opt.MapFrom(src => src.End - DateTime.Now));

        CreateMap<Models.Unverify.UnverifyUserProfile, UnverifyUserProfile>()
            .IncludeBase<Models.Unverify.UnverifyUserProfile, UnverifyInfo>()
            .ForMember(dst => dst.User, opt => opt.MapFrom(src => src.Destination))
            .ForMember(dst => dst.ChannelsToKeep, opt => opt.MapFrom(src => src.ChannelsToKeep.ConvertAll(o => o.ChannelId.ToString())))
            .ForMember(dst => dst.ChannelsToRemove, opt => opt.MapFrom(src => src.ChannelsToRemove.ConvertAll(o => o.ChannelId.ToString())));
    }
}
