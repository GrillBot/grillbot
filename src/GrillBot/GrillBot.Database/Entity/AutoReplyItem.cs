using GrillBot.Database.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

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

    [NotMapped]
    public RegexOptions RegexOptions => RegexOptions.Multiline | (HaveFlags(AutoReplyFlags.CaseSensitive) ? RegexOptions.None : RegexOptions.IgnoreCase);

    public bool HaveFlags(AutoReplyFlags flags)
        => (Flags & (long)flags) != 0;
}
