using Discord;
using Discord.Rest;

namespace GrillBot.Data.Models.AuditLog
{
    public class AuditChannelInfo
    {
        public string Name { get; set; }
        public ChannelType? Type { get; set; }
        public bool? IsNsfw { get; set; }
        public int? Bitrate { get; set; }
        public int? SlowMode { get; set; }
        public string Topic { get; set; }

        public AuditChannelInfo() { }

        public AuditChannelInfo(string name, ChannelType? type, bool? nsfw, int? bitrate, int? slowMode, string topic)
        {
            Name = name;
            Type = type;
            IsNsfw = nsfw;
            Bitrate = bitrate;
            SlowMode = slowMode;
            Topic = topic;
        }

        public AuditChannelInfo(ChannelCreateAuditLogData data)
            : this(data.ChannelName, data.ChannelType, data.IsNsfw, data.Bitrate, data.SlowModeInterval, null) { }

        public AuditChannelInfo(ChannelDeleteAuditLogData data, string topic)
            : this(data.ChannelName, data.ChannelType, data.IsNsfw, data.Bitrate, data.SlowModeInterval, topic) { }

        public AuditChannelInfo(ChannelInfo info)
            : this(info.Name, info.ChannelType, info.IsNsfw, info.Bitrate, info.SlowModeInterval, info.Topic) { }
    }
}
