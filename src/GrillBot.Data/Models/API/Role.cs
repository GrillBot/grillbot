using Discord;

namespace GrillBot.Data.Models.API;

public class Role
{
    /// <summary>
    /// Id of role.
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Role name.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Hexadecimal color of role.
    /// </summary>
    public string Color { get; set; } = "#000000";
}

public class RoleMappingProfile : AutoMapper.Profile
{
    public RoleMappingProfile()
    {
        CreateMap<IRole, Role>();
    }
}