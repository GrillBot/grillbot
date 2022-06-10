using GrillBot.Database.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrillBot.Database.Entity;

public class ExplicitPermission
{
    [StringLength(30)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string TargetId { get; set; } = null!;

    public bool IsRole { get; set; }

    [StringLength(255)]
    public string Command { get; set; } = null!;

    public ExplicitPermissionState State { get; set; }
}
