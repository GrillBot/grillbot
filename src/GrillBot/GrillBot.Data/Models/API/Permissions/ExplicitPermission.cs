using GrillBot.Data.Models.API.Users;
using GrillBot.Database.Enums;

namespace GrillBot.Data.Models.API.Permissions;

public class ExplicitPermission
{
    public string Command { get; set; }
    public User User { get; set; }
    public Role Role { get; set; }
    public ExplicitPermissionState State { get; set; }
}

public class ExplicitPermissionMappingProfile : AutoMapper.Profile
{
    public ExplicitPermissionMappingProfile()
    {
        CreateMap<Database.Entity.ExplicitPermission, ExplicitPermission>();
    }
}