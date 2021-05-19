using GrillBot.Database.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrillBot.Database.Entity
{
    public class Command
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Name { get; set; }

        [Required]
        public long Flags { get; set; } = 0;

        public bool HaveFlags(CommandFlags flags) => (Flags & (long)flags) != 0;
    }
}
