using Discord;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrillBot.Data.Models.AuditLog;

public class AuditLogDataWrapper
{
    public AuditLogItemType Type { get; set; }
    public IGuild Guild { get; set; }
    public IChannel Channel { get; set; }
    public IUser ProcessedUser { get; set; }
    public object Data { get; set; }
    public string DiscordAuditLogItemId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<AuditLogFileMeta> Files { get; set; }

    public ChannelType? ChannelType
        => Channel?.GetChannelType();

    public AuditLogDataWrapper(AuditLogItemType type, object data, IGuild guild = null, IChannel channel = null, IUser processedUser = null,
        string discordAuditLogItemId = null, DateTime? createdAt = null, IEnumerable<AuditLogFileMeta> files = null)
    {
        Type = type;
        Guild = guild;
        Channel = channel;
        ProcessedUser = processedUser;
        DiscordAuditLogItemId = discordAuditLogItemId;
        CreatedAt = createdAt ?? DateTime.Now;
        Files = files != null ? files.ToList() : new List<AuditLogFileMeta>();
        Data = data;
    }

    public AuditLogItem ToEntity(JsonSerializerSettings serializerSettings = null)
    {
        var entity = new AuditLogItem()
        {
            Type = Type,
            ChannelId = Channel?.Id.ToString(),
            CreatedAt = CreatedAt,
            ProcessedUserId = ProcessedUser?.Id.ToString(),
            DiscordAuditLogItemId = DiscordAuditLogItemId,
            GuildId = Guild?.Id.ToString()
        };

        if (Type == AuditLogItemType.Info || Type == AuditLogItemType.Warning || Type == AuditLogItemType.Error)
            entity.Data = Data as string;
        else
            entity.Data = JsonConvert.SerializeObject(Data, serializerSettings);

        Files.ForEach(o => entity.Files.Add(o));
        return entity;
    }
}
