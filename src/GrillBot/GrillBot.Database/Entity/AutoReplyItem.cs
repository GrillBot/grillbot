using GrillBot.Database.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace GrillBot.Database.Entity
{
    public class AutoReplyItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public string Template { get; set; }

        [Required]
        public string Reply { get; set; }

        [Required]
        public long Flags { get; set; }

        [NotMapped]
        public bool IsDisabled => (Flags & (long)AutoReplyFlags.Disabled) != 0;

        [NotMapped]
        public bool CaseSensitive => (Flags & (long)AutoReplyFlags.CaseSensitive) != 0;

        [NotMapped]
        public RegexOptions RegexOptions => RegexOptions.Multiline | (CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
    }
}
