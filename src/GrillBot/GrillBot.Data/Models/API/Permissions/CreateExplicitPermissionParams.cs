using GrillBot.Database.Enums;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Data.Models.API.Permissions;

public class CreateExplicitPermissionParams
{
    [Required]
    public string Command { get; set; }

    [Required]
    public bool IsRole { get; set; }

    [Required]
    public string TargetId { get; set; }

    [Required]
    public ExplicitPermissionState State { get; set; }
}
