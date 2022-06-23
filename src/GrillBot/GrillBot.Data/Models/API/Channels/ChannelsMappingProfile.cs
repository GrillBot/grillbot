using Discord;
using Discord.WebSocket;
using System.Linq;
using GrillBot.Common.Extensions.Discord;

namespace GrillBot.Data.Models.API.Channels;

public class ChannelsMappingProfile : AutoMapper.Profile
{
    public ChannelsMappingProfile()
    {
        CreateMap<IGuildChannel, Channel>()
            .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dst => dst.Type, opt => opt.MapFrom(src => src.GetChannelType()))
            .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.HaveCategory() ? $"{src.Name} ({src.GetCategory().Name})" : src.Name));

        CreateMap<Database.Entity.GuildChannel, Channel>()
            .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.ChannelId))
            .ForMember(dst => dst.Type, opt => opt.MapFrom(src => src.ChannelType));

        CreateMap<Database.Entity.GuildChannel, GuildChannelListItem>()
            .IncludeBase<Database.Entity.GuildChannel, Channel>()
            .ForMember(dst => dst.FirstMessageAt, opt =>
            {
                opt.PreCondition(src => !src.IsCategory() && src.Users.Count > 0);
                opt.MapFrom(src => src.Users.Min(o => o.FirstMessageAt));
            })
            .ForMember(dst => dst.LastMessageAt, opt =>
            {
                opt.PreCondition(src => !src.IsCategory() && src.Users.Count > 0);
                opt.MapFrom(src => src.Users.Min(o => o.LastMessageAt));
            })
            .ForMember(dst => dst.MessagesCount, opt =>
            {
                opt.PreCondition(src => !src.IsCategory() && src.Users.Count > 0);
                opt.MapFrom(src => src.Users.Sum(o => o.Count));
            });

        CreateMap<IGuildChannel, GuildChannelListItem>()
            .ForMember(dst => dst.RolePermissionCount, opt =>
            {
                opt.PreCondition(src => src is not IThreadChannel && src.PermissionOverwrites != null);
                opt.MapFrom(src => src.PermissionOverwrites.Count(o => o.TargetId != src.Guild.EveryoneRole.Id && o.TargetType == PermissionTarget.Role));
            })
            .ForMember(dst => dst.UserPermissionCount, opt =>
            {
                opt.PreCondition(src => src is not IThreadChannel && src.PermissionOverwrites != null);
                opt.MapFrom(src => src.PermissionOverwrites.Count(o => o.TargetType == PermissionTarget.User));
            });

        CreateMap<Database.Entity.GuildChannel, ChannelboardItem>()
            .ForMember(dst => dst.Channel, opt => opt.MapFrom(src => src));

        CreateMap<Database.Entity.GuildChannel, ChannelDetail>()
            .IncludeBase<Database.Entity.GuildChannel, GuildChannelListItem>()
            .ForMember(dst => dst.LastMessageFrom, opt =>
            {
                opt.PreCondition(src => src.Users.Count > 0);
                opt.MapFrom(src => src.Users.OrderByDescending(o => o.LastMessageAt).Select(o => o.User.User).FirstOrDefault());
            })
            .ForMember(dst => dst.MostActiveUser, opt =>
            {
                opt.PreCondition(src => src.Users.Count > 0);
                opt.MapFrom(src => src.Users.OrderByDescending(o => o.Count).Select(o => o.User.User).FirstOrDefault());
            });

        CreateMap<Database.Entity.GuildUserChannel, ChannelUserStatItem>()
            .ForMember(dst => dst.Nickname, opt => opt.MapFrom(src => src.User.Nickname))
            .ForMember(dst => dst.Username, opt => opt.MapFrom(src => src.User.User.Username));

        CreateMap<Database.Entity.GuildUserChannel, UserGuildChannel>();
    }
}
