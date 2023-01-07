using System.ComponentModel.DataAnnotations;

namespace GrillBot.Database.Entity;

public class SelfunverifyKeepable
{
    [StringLength(100)]
    public string GroupName { get; set; } = null!;

    [StringLength(100)]
    public string Name { get; set; } = null!;
}
