using Discord;
using Discord.WebSocket;
using System.Runtime.Serialization;

namespace GrillBot.Data.Models.AuditLog
{
    public class GuildUpdatedData
    {
        public Diff<DefaultMessageNotifications> DefaultMessageNotifications { get; set; }
        public Diff<string> Description { get; set; }
        public Diff<string> VanityUrl { get; set; }
        public bool BannerChanged { get; set; }
        public bool DiscoverySplashChanged { get; set; }
        public bool SplashChanged { get; set; }
        public bool IconChanged { get; set; }
        public Diff<string> VoiceRegionId { get; set; }
        public Diff<AuditUserInfo> Owner { get; set; }
        public Diff<AuditChannelInfo> PublicUpdatesChannel { get; set; }
        public Diff<AuditChannelInfo> RulesChannel { get; set; }
        public Diff<AuditChannelInfo> SystemChannel { get; set; }
        public Diff<AuditChannelInfo> AfkChannel { get; set; }
        public Diff<int> AfkTimeout { get; set; }
        public Diff<string> Name { get; set; }
        public Diff<MfaLevel> MfaLevel { get; set; }

        public GuildUpdatedData() { }

        public GuildUpdatedData(SocketGuild before, SocketGuild after)
        {
            DefaultMessageNotifications = new Diff<DefaultMessageNotifications>(before.DefaultMessageNotifications, after.DefaultMessageNotifications);
            Description = new Diff<string>(before.Description, after.Description);
            VanityUrl = new Diff<string>(before.VanityURLCode, after.VanityURLCode);
            BannerChanged = before.BannerId != after.BannerId;
            DiscoverySplashChanged = before.DiscoverySplashId != after.DiscoverySplashId;
            SplashChanged = before.SplashId != after.SplashId;
            IconChanged = before.IconId != after.IconId;
            VoiceRegionId = new Diff<string>(before.VoiceRegionId, after.VoiceRegionId);
            Owner = new Diff<AuditUserInfo>(new AuditUserInfo(before.Owner), new AuditUserInfo(after.Owner));

            PublicUpdatesChannel = new Diff<AuditChannelInfo>(
                before.PublicUpdatesChannel == null ? null : new AuditChannelInfo(before.PublicUpdatesChannel),
                after.PublicUpdatesChannel == null ? null : new AuditChannelInfo(after.PublicUpdatesChannel)
            );

            RulesChannel = new Diff<AuditChannelInfo>(
                before.RulesChannel == null ? null : new AuditChannelInfo(before.RulesChannel),
                after.RulesChannel == null ? null : new AuditChannelInfo(after.RulesChannel)
            );

            SystemChannel = new Diff<AuditChannelInfo>(
                before.SystemChannel == null ? null : new AuditChannelInfo(before.SystemChannel),
                after.SystemChannel == null ? null : new AuditChannelInfo(after.SystemChannel)
            );

            AfkChannel = new Diff<AuditChannelInfo>(
                before.AFKChannel == null ? null : new AuditChannelInfo(before.AFKChannel),
                after.AFKChannel == null ? null : new AuditChannelInfo(after.AFKChannel)
            );

            AfkTimeout = new Diff<int>(before.AFKTimeout, after.AFKTimeout);
            Name = new Diff<string>(before.Name, after.Name);
            MfaLevel = new Diff<MfaLevel>(before.MfaLevel, after.MfaLevel);
        }

        [OnSerializing]
        internal void OnSerializing(StreamingContext _)
        {
            if (DefaultMessageNotifications?.IsEmpty == true) DefaultMessageNotifications = null;
            if (Description?.IsEmpty == true) Description = null;
            if (VanityUrl?.IsEmpty == true) VanityUrl = null;
            if (VoiceRegionId?.IsEmpty == true) VoiceRegionId = null;
            if (Owner?.IsEmpty == true) Owner = null;
            if (PublicUpdatesChannel?.IsEmpty == true) PublicUpdatesChannel = null;
            if (RulesChannel?.IsEmpty == true) RulesChannel = null;
            if (AfkTimeout?.IsEmpty == true) AfkTimeout = null;
            if (AfkChannel?.IsEmpty == true) AfkChannel = null;
            if (Name?.IsEmpty == true) Name = null;
            if (MfaLevel?.IsEmpty == true) MfaLevel = null;
        }
    }
}
