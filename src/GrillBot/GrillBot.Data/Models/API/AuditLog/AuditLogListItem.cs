using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Data.Models.API.Users;
using GrillBot.Database.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrillBot.Data.Models.API.AuditLog
{
    /// <summary>
    /// Audit log item.
    /// </summary>
    public class AuditLogListItem
    {
        /// <summary>
        /// Item ID.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Datetime of item creation.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Guild
        /// </summary>
        public Guild Guild { get; set; }

        /// <summary>
        /// Processed user.
        /// </summary>
        public GuildUser ProcessedUser { get; set; }

        /// <summary>
        /// Id of auditlog item in discord.
        /// </summary>
        public string DiscordAuditLogItemId { get; set; }

        /// <summary>
        /// Type.
        /// </summary>
        public AuditLogItemType Type { get; set; }

        /// <summary>
        /// Channel where was processed operation.
        /// </summary>
        public GuildChannel Channel { get; set; }

        /// <summary>
        /// Files attached to this log item.
        /// </summary>
        public List<AuditLogFileMetadata> Files { get; set; }

        /// <summary>
        /// This item contains detail?
        /// </summary>
        public bool ContainsData { get; set; }

        public AuditLogListItem() { }

        public AuditLogListItem(Database.Entity.AuditLogItem entity)
        {
            Id = entity.Id;
            CreatedAt = entity.CreatedAt;
            Guild = entity.Guild == null ? null : new(entity.Guild);
            ProcessedUser = entity.ProcessedGuildUser == null ? null : new(entity.ProcessedGuildUser);
            DiscordAuditLogItemId = entity.DiscordAuditLogItemId;
            Type = entity.Type;
            Channel = entity.GuildChannel == null ? null : new(entity.GuildChannel);
            ContainsData = !string.IsNullOrEmpty(entity.Data);
            Files = entity.Files.Select(o => new AuditLogFileMetadata(o)).ToList();
        }
    }
}
