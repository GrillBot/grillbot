using Discord;
using GrillBot.Core.Models;

namespace GrillBot.Common.Services.AuditLog.Models.Response.Detail;

public class GuildUpdatedDetail
{
    public Diff<DefaultMessageNotifications>? DefaultMessageNotifications { get; set; } 
    public Diff<string?>? Description { get; set; } 
    public Diff<string?>? VanityUrl { get; set; } 
    public Diff<string?>? BannerId { get; set; } 
    public Diff<string?>? DiscoverySplashId { get; set; } 
    public Diff<string?>? SplashId { get; set; } 
    public Diff<string?>? IconId { get; set; } 
    public Diff<byte[]?>? IconData { get; set; } 
    public Diff<string?>? PublicUpdatesChannelId { get; set; } 
    public Diff<string?>? RulesChannelId { get; set; } 
    public Diff<string?>? SystemChannelId { get; set; } 
    public Diff<string?>? AfkChannelId { get; set; } 
    public Diff<int>? AfkTimeout { get; set; } 
    public Diff<string>? Name { get; set; } 
    public Diff<MfaLevel>? MfaLevel { get; set; } 
    public Diff<VerificationLevel>? VerificationLevel { get; set; } 
    public Diff<ExplicitContentFilterLevel>? ExplicitContentFilter { get; set; } 
    public Diff<GuildFeature>? Features { get; set; } 
    public Diff<PremiumTier>? PremiumTier { get; set; } 
    public Diff<SystemChannelMessageDeny>? SystemChannelFlags { get; set; } 
    public Diff<NsfwLevel>? NsfwLevel { get; set; }
}
