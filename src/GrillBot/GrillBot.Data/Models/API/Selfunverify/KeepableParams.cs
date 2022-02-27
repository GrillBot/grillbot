using System.ComponentModel.DataAnnotations;

namespace GrillBot.Data.Models.API.Selfunverify;

public class KeepableParams
{
    public string Group { get; set; } = "_";

    [Required]
    public string Name { get; set; }
}
