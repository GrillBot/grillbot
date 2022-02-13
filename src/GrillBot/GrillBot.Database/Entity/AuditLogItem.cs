using Discord;
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

        [StringLength(30)]
        public string GuildId { get; set; }

        [ForeignKey(nameof(GuildId))]
        public Guild Guild { get; set; }

        [StringLength(30)]
        public string ProcessedUserId { get; set; }

        public GuildUser ProcessedGuildUser { get; set; }

        public User ProcessedUser { get; set; }

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

        public static AuditLogItem Create(AuditLogItemType type, IGuild guild, IChannel channel, IUser processedUser, string data, string discordAuditLogItemId, DateTime? createdAt)
        {
            return new AuditLogItem()
            {
                ChannelId = guild != null ? channel?.Id.ToString() : null,
                CreatedAt = createdAt ?? DateTime.Now,
                Data = data,
                DiscordAuditLogItemId = discordAuditLogItemId,
                GuildId = guild?.Id.ToString(),
                ProcessedUserId = processedUser?.Id.ToString(),
                Type = type
            };
        }

        public static AuditLogItem Create(AuditLogItemType type, IGuild guild, IChannel channel, IUser processedUser, string data, ulong? discordAuditLogItemId, DateTime? createdAt)
        {
            return Create(type, guild, channel, processedUser, data, discordAuditLogItemId?.ToString(), createdAt);
        }

        public static AuditLogItem Create(AuditLogItemType type, IGuild guild, IChannel channel, IUser processedUser, string data, DateTime? createdAt)
        {
            return Create(type, guild, channel, processedUser, data, null as string, createdAt);
        }
    }
}
