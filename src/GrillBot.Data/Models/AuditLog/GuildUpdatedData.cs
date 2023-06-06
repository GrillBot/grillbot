using Discord;
using System.Runtime.Serialization;
using GrillBot.Core.Models;

namespace GrillBot.Data.Models.AuditLog;

public class GuildUpdatedData
{
    public Diff<DefaultMessageNotifications>? DefaultMessageNotifications { get; set; }
    public Diff<string>? Description { get; set; }
    public Diff<string>? VanityUrl { get; set; }
    public bool BannerChanged { get; set; }
    public bool DiscoverySplashChanged { get; set; }
    public bool SplashChanged { get; set; }
    public bool IconChanged { get; set; }
    public Diff<string>? VoiceRegionId { get; set; }
    public Diff<AuditUserInfo>? Owner { get; set; }
    public Diff<AuditChannelInfo?>? PublicUpdatesChannel { get; set; }
    public Diff<AuditChannelInfo?>? RulesChannel { get; set; }
    public Diff<AuditChannelInfo?>? SystemChannel { get; set; }
    public Diff<AuditChannelInfo?>? AfkChannel { get; set; }
    public Diff<int>? AfkTimeout { get; set; }
    public Diff<string>? Name { get; set; }
    public Diff<MfaLevel>? MfaLevel { get; set; }
    public Diff<VerificationLevel>? VerificationLevel { get; set; }
    public Diff<ExplicitContentFilterLevel>? ExplicitContentFilter { get; set; }
    public Diff<GuildFeature>? Features { get; set; }
    public Diff<PremiumTier>? PremiumTier { get; set; }
    public Diff<SystemChannelMessageDeny>? SystemChannelFlags { get; set; }
    public Diff<NsfwLevel>? NsfwLevel { get; set; }

    [OnSerializing]
    internal void OnSerializing(StreamingContext _)
    {
        if (DefaultMessageNotifications?.IsEmpty() == true) DefaultMessageNotifications = null;
        if (Description?.IsEmpty() == true) Description = null;
        if (VanityUrl?.IsEmpty() == true) VanityUrl = null;
        if (VoiceRegionId?.IsEmpty() == true) VoiceRegionId = null;
        if (Owner?.IsEmpty() == true) Owner = null;
        if (PublicUpdatesChannel?.IsEmpty() == true) PublicUpdatesChannel = null;
        if (RulesChannel?.IsEmpty() == true) RulesChannel = null;
        if (SystemChannel?.IsEmpty() == true) SystemChannel = null;
        if (AfkTimeout?.IsEmpty() == true) AfkTimeout = null;
        if (AfkChannel?.IsEmpty() == true) AfkChannel = null;
        if (Name?.IsEmpty() == true) Name = null;
        if (MfaLevel?.IsEmpty() == true) MfaLevel = null;
        if (VerificationLevel?.IsEmpty() == true) VerificationLevel = null;
        if (ExplicitContentFilter?.IsEmpty() == true) ExplicitContentFilter = null;
        if (Features?.IsEmpty() == true) Features = null;
        if (PremiumTier?.IsEmpty() == true) PremiumTier = null;
        if (SystemChannelFlags?.IsEmpty() == true) SystemChannelFlags = null;
        if (NsfwLevel?.IsEmpty() == true) NsfwLevel = null;
    }
}
