using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrillBot.Database.Entity
{
    public class Unverify
    {
        [StringLength(30)]
        public string GuildId { get; set; }

        [StringLength(30)]
        public string UserId { get; set; }

        public GuildUser GuildUser { get; set; }

        [Required]
        public DateTime StartAt { get; set; }

        [Required]
        public DateTime EndAt { get; set; }

        public string Reason { get; set; }

        [Column(TypeName = "jsonb")]
        public List<string> Roles { get; set; }

        [Column(TypeName = "jsonb")]
        public List<string> Channels { get; set; }

        public long SetOperationId { get; set; }

        [ForeignKey(nameof(SetOperationId))]
        public UnverifyLog UnverifyLog { get; set; }
    }
}
