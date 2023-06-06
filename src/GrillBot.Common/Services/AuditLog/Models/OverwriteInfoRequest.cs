namespace GrillBot.Common.Services.AuditLog.Models;

public class OverwriteInfoRequest
{
    /// <summary>
    /// ID of audit log record in the discord.
    /// </summary>
    public string DiscordLogId { get; set; } = null!;
}
