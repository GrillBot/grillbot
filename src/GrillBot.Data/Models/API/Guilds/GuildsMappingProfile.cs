using Discord;
using Discord.WebSocket;
using GrillBot.Common.Extensions.Discord;

namespace GrillBot.Data.Models.API.Guilds;

public class GuildsMappingProfile : AutoMapper.Profile
{
    public GuildsMappingProfile()
    {
        CreateMap<Database.Entity.Guild, Guild>();

        CreateMap<SocketGuild, Guild>()
            .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id.ToString()));

        CreateMap<IGuild, Guild>()
            .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dst => dst.MemberCount, opt => opt.MapFrom(src => src.GetMemberCount()));

        CreateMap<Database.Entity.Guild, GuildDetail>()
            .IncludeBase<Database.Entity.Guild, Guild>();

        CreateMap<SocketGuild, GuildDetail>()
            .ForMember(dst => dst.Owner, opt => opt.MapFrom(src => src.Owner))
            .ForMember(dst => dst.VanityUrl, opt =>
            {
                opt.PreCondition(src => !string.IsNullOrEmpty(src.VanityURLCode));
                opt.MapFrom(src => DiscordConfig.InviteUrl + src.VanityURLCode);
            })
            .ForMember(dst => dst.MaxBitrate, opt => opt.MapFrom(src => src.MaxBitrate / 1000))
            .ForMember(dst => dst.MaxUploadLimit, opt => opt.MapFrom(src => src.CalculateFileUploadLimit()));

        CreateMap<SocketGuild, Guild>();
    }
}
