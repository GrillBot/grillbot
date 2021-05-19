using GrillBot.Database.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrillBot.Database.Entity
{
    public class UnverifyLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public UnverifyOperation Operation { get; set; }

        [Required]
        [StringLength(30)]
        public string GuildId { get; set; }

        [Required]
        [StringLength(30)]
        public string FromUserId { get; set; }

        public GuildUser FromUser { get; set; }

        [Required]
        [StringLength(30)]
        public string ToUserId { get; set; }

        public GuildUser ToUser { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public string Data { get; set; }

        public Unverify Unverify { get; set; }
    }
}