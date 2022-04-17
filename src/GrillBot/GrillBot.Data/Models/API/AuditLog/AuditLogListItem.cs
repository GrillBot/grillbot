using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Data.Models.API.Users;
using GrillBot.Database.Enums;
using System;
using System.Collections.Generic;

namespace GrillBot.Data.Models.API.AuditLog;

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
}

public class AuditLogListItemMappingProfile : AutoMapper.Profile
{
    public AuditLogListItemMappingProfile()
    {
        CreateMap<Database.Entity.AuditLogItem, AuditLogListItem>()
            .ForMember(dst => dst.DiscordAuditLogItemIds, opt => opt.MapFrom(src => src.DiscordAuditLogItemId.Split(',', StringSplitOptions.RemoveEmptyEntries)))
            .ForMember(dst => dst.ProcessedUser, opt =>
            {
                opt.PreCondition(o => o.ProcessedUser != null || o.ProcessedGuildUser != null);
                opt.MapFrom(src => src.ProcessedGuildUser != null ? src.ProcessedGuildUser.User : src.ProcessedUser);
            })
            .ForMember(dst => dst.Data, opt => opt.Ignore());
    }
}
