using GrillBot.Data.Models.API.Guilds;
using GrillBot.Data.Models.API.Users;
using GrillBot.Database.Enums;
using System;

namespace GrillBot.Data.Models.API.Unverify;

/// <summary>
/// Unverify log item.
/// </summary>
public class UnverifyLogItem
{
    /// <summary>
    /// Log ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Operation.
    /// </summary>
    public UnverifyOperation Operation { get; set; }

    /// <summary>
    /// Guild where unverify was processed.
    /// </summary>
    public Guild Guild { get; set; }

    /// <summary>
    /// Who did unverify operation.
    /// </summary>
    public GuildUser FromUser { get; set; }

    /// <summary>
    /// Who was target of operation.
    /// </summary>
    public GuildUser ToUser { get; set; }

    /// <summary>
    /// When was log item created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Data of item if operation is Remove, AutoRemove or Recovery.
    /// </summary>
    public UnverifyLogRemove RemoveData { get; set; }

    /// <summary>
    /// Data of item if operation is Unverify or SelfUnverify.
    /// </summary>
    public UnverifyLogSet SetData { get; set; }

    /// <summary>
    /// Data of item if operation is Update.
    /// </summary>
    public UnverifyLogUpdate UpdateData { get; set; }
}
