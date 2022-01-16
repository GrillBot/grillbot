using Discord;
using Discord.Rest;

namespace GrillBot.Data.Models.AuditLog
{
    public class AuditChannelInfo : AuditChannelBaseInfo
    {
        public ChannelType? Type { get; set; }
        public bool? IsNsfw { get; set; }
        public int? Bitrate { get; set; }
        public string Topic { get; set; }

        public AuditChannelInfo() { }

        public AuditChannelInfo(ulong id, string name, ChannelType? type, bool? nsfw, int? bitrate, int? slowMode, string topic)
            : base(id, name, slowMode)
        {
            Type = type;
            IsNsfw = nsfw;
            Bitrate = bitrate;
            Topic = topic;
        }

        public AuditChannelInfo(ChannelCreateAuditLogData data)
            : this(data.ChannelId, data.ChannelName, data.ChannelType, data.IsNsfw, data.Bitrate, data.SlowModeInterval, null) { }

        public AuditChannelInfo(ChannelDeleteAuditLogData data, string topic)
            : this(data.ChannelId, data.ChannelName, data.ChannelType, data.IsNsfw, data.Bitrate, data.SlowModeInterval, topic) { }

        public AuditChannelInfo(ulong id, ChannelInfo info)
            : this(id, info.Name, info.ChannelType, info.IsNsfw, info.Bitrate, info.SlowModeInterval, info.Topic) { }

        public AuditChannelInfo(IChannel channel)
            : this(channel.Id, channel.Name, channel is IVoiceChannel ? ChannelType.Voice : ChannelType.Text, (channel as ITextChannel)?.IsNsfw,
                  (channel as IVoiceChannel)?.Bitrate, (channel as ITextChannel)?.SlowModeInterval, (channel as ITextChannel)?.Topic)
        { }
    }
}
