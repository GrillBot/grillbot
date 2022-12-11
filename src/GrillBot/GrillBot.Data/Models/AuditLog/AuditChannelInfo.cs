using Discord;
using Discord.Rest;
using GrillBot.Database.Entity;

namespace GrillBot.Data.Models.AuditLog;

public class AuditChannelInfo : AuditChannelBaseInfo
{
    public ChannelType? Type { get; set; }
    public bool? IsNsfw { get; set; }
    public int? Bitrate { get; set; }
    public string Topic { get; set; }
    public long Flags { get; set; }
    public int Position { get; set; } = -1;

    public AuditChannelInfo()
    {
    }

    public AuditChannelInfo(ulong id, string name, ChannelType? type, bool? nsfw, int? bitrate, int? slowMode, string topic, int position)
        : base(id, name, slowMode)
    {
        Type = type;
        IsNsfw = nsfw;
        Bitrate = bitrate;
        Topic = topic;
        Position = position;
    }

    public AuditChannelInfo(ChannelCreateAuditLogData data, IGuildChannel channel)
        : this(data.ChannelId, data.ChannelName, data.ChannelType, data.IsNsfw, data.Bitrate, data.SlowModeInterval, (channel as ITextChannel)?.Topic, channel.Position)
    {
    }

    public AuditChannelInfo(ChannelDeleteAuditLogData data, IGuildChannel channel)
        : this(data.ChannelId, data.ChannelName, data.ChannelType, data.IsNsfw, data.Bitrate, data.SlowModeInterval, (channel as ITextChannel)?.Topic, channel.Position)
    {
    }

    public AuditChannelInfo(ulong id, ChannelInfo info, IGuildChannel channel)
        : this(id, info.Name, info.ChannelType, info.IsNsfw, info.Bitrate, info.SlowModeInterval, info.Topic, channel.Position)
    {
    }

    public AuditChannelInfo(IChannel channel)
        : this(channel.Id, channel.Name, channel is IVoiceChannel ? ChannelType.Voice : ChannelType.Text, (channel as ITextChannel)?.IsNsfw,
            (channel as IVoiceChannel)?.Bitrate, (channel as ITextChannel)?.SlowModeInterval, (channel as ITextChannel)?.Topic, (channel as IGuildChannel)?.Position ?? -1)
    {
    }

    public AuditChannelInfo(GuildChannel channel)
    {
        Flags = channel.Flags;
    }
}
