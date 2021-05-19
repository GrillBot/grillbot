using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrillBot.Database.Entity
{
    public class GuildChannel
    {
        [Key]
        [StringLength(30)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [StringLength(30)]
        public string GuildId { get; set; }

        [ForeignKey(nameof(GuildId))]
        public Guild Guild { get; set; }

        [StringLength(30)]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [Required]
        public long Count { get; set; } = 0;

        [Required]
        public DateTime FirstMessageAt { get; set; }

        [Required]
        public DateTime LastMessageAt { get; set; }

        public ISet<SearchItem> SearchItems { get; set; }

        public GuildChannel()
        {
            SearchItems = new HashSet<SearchItem>();
        }
    }
}
