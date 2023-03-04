using System.ComponentModel.DataAnnotations;

namespace GrillBot.Database.Enums;

public enum AuditLogItemType
{
    None = 0,

    /// <summary>
    /// Information text.
    /// </summary>
    Info = 1,

    /// <summary>
    /// Warning text.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Errors
    /// </summary>
    Error = 3,

    /// <summary>
    /// Executed command.
    /// </summary>
    Command = 4,

    /// <summary>
    /// Channel created.
    /// </summary>
    ChannelCreated = 5,

    /// <summary>
    /// Channel deleted.
    /// </summary>
    ChannelDeleted = 6,

    /// <summary>
    /// Channel updated.
    /// </summary>
    ChannelUpdated = 7,

    /// <summary>
    /// Emote deleted.
    /// </summary>
    EmojiDeleted = 8,

    /// <summary>
    /// Channel overwrite created.
    /// </summary>
    OverwriteCreated = 9,

    /// <summary>
    /// Channel overwrite deleted.
    /// </summary>
    OverwriteDeleted = 10,

    /// <summary>
    /// Channel overwrite updated.
    /// </summary>
    OverwriteUpdated = 11,

    /// <summary>
    /// User unbanned.
    /// </summary>
    Unban = 12,

    /// <summary>
    /// User updated.
    /// </summary>
    MemberUpdated = 13,

    /// <summary>
    /// Member role updated.
    /// </summary>
    MemberRoleUpdated = 14,

    /// <summary>
    /// Guild updated.
    /// </summary>
    GuildUpdated = 15,

    /// <summary>
    /// User left guild.
    /// </summary>
    UserLeft = 16,

    /// <summary>
    /// User joined guild.
    /// </summary>
    UserJoined = 17,

    /// <summary>
    /// Message modified.
    /// </summary>
    MessageEdited = 18,

    /// <summary>
    /// Message removed.
    /// </summary>
    MessageDeleted = 19,

    /// <summary>
    /// Interaction command
    /// </summary>
    InteractionCommand = 20,

    /// <summary>
    /// Thread was deleted.
    /// </summary>
    ThreadDeleted = 21,

    /// <summary>
    /// Quartz job finished.
    /// </summary>
    JobCompleted = 22,

    /// <summary>
    /// API request.
    /// </summary>
    Api = 23,

    /// <summary>
    /// Thread was modified.
    /// </summary>
    ThreadUpdated = 24
}
