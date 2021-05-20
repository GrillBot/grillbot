using GrillBot.Database.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrillBot.Database.Entity
{
    public class AuditLogItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        [StringLength(30)]
        public string GuildId { get; set; }

        [StringLength(30)]
        public string ProcessedUserId { get; set; }

        public GuildUser ProcessedGuildUser { get; set; }

        [StringLength(30)]
        public string DiscordAuditLogItemId { get; set; }

        public string Data { get; set; }

        public AuditLogItemType Type { get; set; }

        [StringLength(30)]
        public string ChannelId { get; set; }

        public GuildChannel GuildChannel { get; set; }

        public ISet<AuditLogFileMeta> Files { get; set; }

        public AuditLogItem()
        {
            Files = new HashSet<AuditLogFileMeta>();
        }
    }
}
