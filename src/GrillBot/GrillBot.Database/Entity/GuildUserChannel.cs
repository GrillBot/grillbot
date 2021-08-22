using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrillBot.Database.Entity
{
    public class GuildUserChannel
    {
        [StringLength(30)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        public GuildChannel Channel { get; set; }

        [StringLength(30)]
        public string GuildId { get; set; }

        public Guild Guild { get; set; }

        [StringLength(30)]
        public string UserId { get; set; }

        public GuildUser User { get; set; }

        [Required]
        public long Count { get; set; } = 0;

        [Required]
        public DateTime FirstMessageAt { get; set; }

        [Required]
        public DateTime LastMessageAt { get; set; }
    }
}
