using System.ComponentModel.DataAnnotations;
using Discord;

namespace AuditLogService.Models.Request;

public class ChannelInfoRequest
{
    [Required]
    public string ChannelName { get; set; } = null!;

    public int? SlowMode { get; set; }

    [Required]
    public ChannelType ChannelType { get; set; }

    [Required]
    public bool IsNsfw { get; set; }

    public int? Bitrate { get; set; }

    public string? Topic { get; set; }

    [Required]
    public int Position { get; set; }
}
