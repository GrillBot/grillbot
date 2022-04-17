namespace GrillBot.Data.Models.API.Invites;

public class InvitesMappingProfile : AutoMapper.Profile
{
    public InvitesMappingProfile()
    {
        CreateMap<Database.Entity.Invite, InviteBase>();

        CreateMap<Database.Entity.Invite, Invite>()
            .ForMember(dst => dst.Creator, opt => opt.MapFrom(src => src.Creator.User))
            .ForMember(dst => dst.UsedUsersCount, opt => opt.MapFrom(src => src.UsedUsers.Count));

        CreateMap<Database.Entity.Invite, GuildInvite>()
            .IncludeBase<Database.Entity.Invite, Invite>();
    }
}
