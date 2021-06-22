using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrillBot.Database.Entity
{
    public class EmoteStatisticItem
    {
        [StringLength(255)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string EmoteId { get; set; }

        [StringLength(30)]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [Required]
        public long UseCount { get; set; } = 0;

        [Required]
        public DateTime FirstOccurence { get; set; }

        [Required]
        public DateTime LastOccurence { get; set; }
    }
}
