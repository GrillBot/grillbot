using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Validation;

namespace GrillBot.Data.Models.API.AuditLog.Public;

/// <summary>
/// Request for creation text based log item from third party service.
/// </summary>
public class LogMessageRequest : IValidatableObject, IDictionaryObject
{
    /// <summary>
    /// Type of log message. Allowed is only "warning", "info" or "error".
    /// </summary>
    [Required]
    [StringLength(32)]
    public string Type { get; set; } = null!;

    [StringLength(32)]
    [DiscordId]
    public string? ChannelId { get; set; }

    /// <summary>
    /// Mandatory if ChannelId was filled.
    /// </summary>
    [StringLength(32)]
    [DiscordId]
    public string? GuildId { get; set; }

    /// <summary>
    /// Log message. For example stack tracing, but can it anything.
    /// </summary>
    [Required]
    public string Message { get; set; } = null!;

    /// <summary>
    /// The source of message. For example name of event.
    /// </summary>
    [Required]
    public string MessageSource { get; set; } = null!;

    [StringLength(32)]
    [DiscordId]
    public string? UserId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var allowedTypes = new[] { "warning", "info", "error" };
        if (!allowedTypes.Contains(Type.ToLower()))
            yield return new ValidationResult("Unallowed type of log item. Allowed is only \"Info\", \"Warning\" or \"Error\" values", new[] { nameof(Type) });

        if (!string.IsNullOrEmpty(ChannelId) && string.IsNullOrEmpty(GuildId))
            yield return new ValidationResult("Guild ID is mandatory if channel ID was filled.", new[] { nameof(ChannelId), nameof(GuildId) });
    }

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(Type), Type },
            { nameof(ChannelId), ChannelId },
            { nameof(GuildId), GuildId },
            { "MessageLength", Message.Length.ToString() },
            { nameof(MessageSource), MessageSource },
            { nameof(UserId), UserId }
        };
    }
}
