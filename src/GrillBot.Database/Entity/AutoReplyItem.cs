using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrillBot.Database.Entity;

public class AutoReplyItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    public string Template { get; set; } = null!;

    [Required]
    public string Reply { get; set; } = null!;

    [Required]
    public long Flags { get; set; }
}
