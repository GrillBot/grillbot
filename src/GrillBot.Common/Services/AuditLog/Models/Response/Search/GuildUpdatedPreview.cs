namespace GrillBot.Common.Services.AuditLog.Models.Response.Search;

public class GuildUpdatedPreview
{
    public bool DefaultMessageNotifications { get; set; }
    public bool Description { get; set; }
    public bool VanityUrl { get; set; }
    public bool BannerId { get; set; }
    public bool DiscoverySplashId { get; set; }
    public bool SplashId { get; set; }
    public bool IconId { get; set; }
    public bool PublicUpdatesChannelId { get; set; }
    public bool RulesChannelId { get; set; }
    public bool SystemChannelId { get; set; }
    public bool AfkChannelId { get; set; }
    public bool AfkTimeout { get; set; }
    public bool Name { get; set; }
    public bool MfaLevel { get; set; }
    public bool VerificationLevel { get; set; }
    public bool ExplicitContentFilter { get; set; }
    public bool Features { get; set; }
    public bool PremiumTier { get; set; }
    public bool SystemChannelFlags { get; set; }
    public bool NsfwLevel { get; set; }
}
