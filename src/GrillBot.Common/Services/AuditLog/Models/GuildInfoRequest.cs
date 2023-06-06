using System.ComponentModel.DataAnnotations;
using Discord;

namespace AuditLogService.Models.Request;

public class GuildInfoRequest
{
    public DefaultMessageNotifications DefaultMessageNotifications { get; set; }
    public string? Description { get; set; }
    public string? VanityUrl { get; set; }
    public string? BannerId { get; set; }
    public string? DiscoverySplashId { get; set; }
    public string? SplashId { get; set; }
    public string? IconId { get; set; }
    public byte[]? IconData { get; set; }
    public string? VoiceRegionId { get; set; }

    [Required]
    [StringLength(32)]
    public string OwnerId { get; set; } = null!;

    [StringLength(32)]
    public string? PublicUpdatesChannelId { get; set; }

    [StringLength(32)]
    public string? RulesChannelId { get; set; }

    [StringLength(32)]
    public string? SystemChannelId { get; set; }

    [StringLength(32)]
    public string? AfkChannelId { get; set; }

    public int AfkTimeout { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    public MfaLevel MfaLevel { get; set; }
    public VerificationLevel VerificationLevel { get; set; }
    public ExplicitContentFilterLevel ExplicitContentFilter { get; set; }
    public GuildFeature Features { get; set; }
    public PremiumTier PremiumTier { get; set; }
    public SystemChannelMessageDeny SystemChannelFlags { get; set; }
    public NsfwLevel NsfwLevel { get; set; }
}
