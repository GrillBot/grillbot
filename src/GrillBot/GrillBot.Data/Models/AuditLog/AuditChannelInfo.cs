using Discord;
using Discord.Rest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GrillBot.Data.Models.AuditLog
{
    public class AuditChannelInfo : IComparable
    {
        [JsonIgnore]
        public ulong Id { get; set; }
        public string Name { get; set; }
        public ChannelType? Type { get; set; }
        public bool? IsNsfw { get; set; }
        public int? Bitrate { get; set; }
        public int? SlowMode { get; set; }
        public string Topic { get; set; }

        public AuditChannelInfo() { }

        public AuditChannelInfo(ulong id, string name, ChannelType? type, bool? nsfw, int? bitrate, int? slowMode, string topic)
        {
            Id = id;
            Name = name;
            Type = type;
            IsNsfw = nsfw;
            Bitrate = bitrate;
            SlowMode = slowMode;
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

        public int CompareTo(object obj) => obj is AuditChannelInfo channel && channel.Id == Id ? 0 : 1;
        public override bool Equals(object obj) => CompareTo(obj) == 0;
        public override int GetHashCode() => HashCode.Combine(Id);
        public static bool operator ==(AuditChannelInfo left, AuditChannelInfo right) => EqualityComparer<AuditChannelInfo>.Default.Equals(left, right);
        public static bool operator !=(AuditChannelInfo left, AuditChannelInfo right) => left != right;
        public static bool operator >(AuditChannelInfo left, AuditChannelInfo right) => left.CompareTo(right) != 0;
        public static bool operator <(AuditChannelInfo left, AuditChannelInfo right) => left.CompareTo(right) != 0;
        public static bool operator <=(AuditChannelInfo left, AuditChannelInfo right) => left.CompareTo(right) != 0;
        public static bool operator >=(AuditChannelInfo left, AuditChannelInfo right) => left.CompareTo(right) != 0;
    }
}
