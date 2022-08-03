using Discord;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Database.Enums;
using System.Linq;

namespace GrillBot.Data.Models.API.Users;

public class UsersMappingProfile : AutoMapper.Profile
{
    public UsersMappingProfile()
    {
        CreateMap<IUser, User>()
            .ForMember(dst => dst.AvatarUrl, opt => opt.MapFrom(src => src.GetUserAvatarUrl(128)));

        CreateMap<Database.Entity.User, User>()
            .ForMember(dst => dst.IsBot, opt => opt.MapFrom(src => src.HaveFlags(UserFlags.NotUser)));

        CreateMap<Database.Entity.GuildUser, GuildUser>()
            .ForMember(o => o.Id, opt => opt.MapFrom(src => src.User.Id))
            .ForMember(o => o.Username, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(o => o.Discriminator, opt => opt.MapFrom(src => src.User.Discriminator))
            .ForMember(o => o.IsBot, opt => opt.MapFrom(src => src.User.HaveFlags(UserFlags.NotUser)));

        CreateMap<Database.Models.Points.PointBoardItem, UserPointsItem>()
            .ForMember(o => o.Guild, opt => opt.MapFrom(src => src.GuildUser.Guild))
            .ForMember(o => o.User, opt => opt.MapFrom(src => src.GuildUser.User))
            .ForMember(o => o.Nickname, opt => opt.MapFrom(src => src.GuildUser.Nickname));

        CreateMap<Database.Entity.User, UserListItem>()
            .ForMember(dst => dst.HaveBirthday, opt => opt.MapFrom(src => src.Birthday != null))
            .ForMember(dst => dst.Username, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Discriminator) ? src.Username : $"{src.Username}#{src.Discriminator}"))
            .ForMember(dst => dst.Guilds, opt => opt.Ignore())
            .ForMember(dst => dst.DiscordStatus, opt => opt.MapFrom(src => src.Status))
            .ForMember(dst => dst.RegisteredAt, opt => opt.MapFrom(src => SnowflakeUtils.FromSnowflake(src.Id.ToUlong()).LocalDateTime));

        CreateMap<Database.Entity.User, UserDetail>()
            .ForMember(dst => dst.HaveBirthday, opt => opt.MapFrom(src => src.Birthday != null))
            .ForMember(dst => dst.AvatarUrl, opt => opt.MapFrom(_ => CDN.GetDefaultUserAvatarUrl(0)))
            .ForMember(dst => dst.Guilds, opt => opt.Ignore())
            .ForMember(dst => dst.RegisteredAt, opt => opt.MapFrom(src => SnowflakeUtils.FromSnowflake(src.Id.ToUlong()).LocalDateTime));

        CreateMap<IUser, UserDetail>()
            .ForMember(dst => dst.IsKnown, opt => opt.MapFrom(_ => true))
            .ForMember(dst => dst.ActiveClients, opt => opt.MapFrom(src => src.ActiveClients.Select(o => o.ToString()).OrderBy(o => o).ToList()))
            .ForMember(dst => dst.AvatarUrl, opt => opt.MapFrom(src => src.GetUserAvatarUrl(128)));

        CreateMap<Database.Entity.GuildUser, GuildUserDetail>()
            .ForMember(dst => dst.Emotes, opt => opt.MapFrom(src => src.EmoteStatistics))
            .ForMember(dst => dst.Unverify, opt => opt.Ignore())
            .ForMember(dst => dst.Points, opt => opt.Ignore());
    }
}
