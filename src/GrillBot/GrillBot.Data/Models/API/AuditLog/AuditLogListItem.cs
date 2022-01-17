using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Data.Models.API.Users;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using Newtonsoft.Json;
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
        public User ProcessedUser { get; set; }

        /// <summary>
        /// Id of auditlog items in discord.
        /// </summary>
        public List<string> DiscordAuditLogItemIds { get; set; }

        /// <summary>
        /// Type.
        /// </summary>
        public AuditLogItemType Type { get; set; }

        /// <summary>
        /// Channel where was processed operation.
        /// </summary>
        public Channel Channel { get; set; }

        /// <summary>
        /// Files attached to this log item.
        /// </summary>
        public List<AuditLogFileMetadata> Files { get; set; }

        /// <summary>
        /// Data
        /// </summary>
        public object Data { get; set; }

        public AuditLogListItem() { }

        public AuditLogListItem(Database.Entity.AuditLogItem entity, JsonSerializerSettings jsonSerializerSettings)
        {
            Id = entity.Id;
            CreatedAt = entity.CreatedAt;
            Guild = entity.Guild == null ? null : new(entity.Guild);
            DiscordAuditLogItemIds = !string.IsNullOrEmpty(entity.DiscordAuditLogItemId) ? entity.DiscordAuditLogItemId.Split(',').ToList() : null;
            Type = entity.Type;
            Channel = entity.GuildChannel == null ? null : new(entity.GuildChannel);
            Files = entity.Files.Select(o => new AuditLogFileMetadata(o)).ToList();

            if (entity.ProcessedGuildUser != null)
                ProcessedUser = new(entity.ProcessedGuildUser.User);
            else if (entity.ProcessedUser != null)
                ProcessedUser = new(entity.ProcessedUser);

            if (!string.IsNullOrEmpty(entity.Data))
            {
                Data = entity.Type switch
                {
                    AuditLogItemType.Error or AuditLogItemType.Info or AuditLogItemType.Warning => entity.Data,
                    AuditLogItemType.Command => JsonConvert.DeserializeObject<CommandExecution>(entity.Data, jsonSerializerSettings),
                    AuditLogItemType.ChannelCreated or AuditLogItemType.ChannelDeleted => JsonConvert.DeserializeObject<AuditChannelInfo>(entity.Data, jsonSerializerSettings),
                    AuditLogItemType.ChannelUpdated => JsonConvert.DeserializeObject<Diff<AuditChannelInfo>>(entity.Data, jsonSerializerSettings),
                    AuditLogItemType.EmojiDeleted => JsonConvert.DeserializeObject<AuditEmoteInfo>(entity.Data, jsonSerializerSettings),
                    AuditLogItemType.GuildUpdated => JsonConvert.DeserializeObject<GuildUpdatedData>(entity.Data, jsonSerializerSettings),
                    AuditLogItemType.MemberRoleUpdated or AuditLogItemType.MemberUpdated => JsonConvert.DeserializeObject<MemberUpdatedData>(entity.Data, jsonSerializerSettings),
                    AuditLogItemType.MessageDeleted => JsonConvert.DeserializeObject<MessageDeletedData>(entity.Data, jsonSerializerSettings),
                    AuditLogItemType.MessageEdited => JsonConvert.DeserializeObject<MessageEditedData>(entity.Data, jsonSerializerSettings),
                    AuditLogItemType.OverwriteCreated or AuditLogItemType.OverwriteDeleted => JsonConvert.DeserializeObject<AuditOverwriteInfo>(entity.Data, jsonSerializerSettings),
                    AuditLogItemType.OverwriteUpdated => JsonConvert.DeserializeObject<Diff<AuditOverwriteInfo>>(entity.Data, jsonSerializerSettings),
                    AuditLogItemType.Unban => JsonConvert.DeserializeObject<AuditUserInfo>(entity.Data, jsonSerializerSettings),
                    AuditLogItemType.UserJoined => JsonConvert.DeserializeObject<UserJoinedAuditData>(entity.Data, jsonSerializerSettings),
                    AuditLogItemType.UserLeft => JsonConvert.DeserializeObject<UserLeftGuildData>(entity.Data, jsonSerializerSettings),
                    AuditLogItemType.InteractionCommand => JsonConvert.DeserializeObject<InteractionCommandExecuted>(entity.Data, jsonSerializerSettings),
                    AuditLogItemType.ThreadDeleted => JsonConvert.DeserializeObject<AuditThreadInfo>(entity.Data, jsonSerializerSettings),
                    _ => null
                };
            }
        }
    }
}
