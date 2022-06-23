using Discord;
using GrillBot.Database.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace GrillBot.Database.Entity;

[DebuggerDisplay("{Name} ({ChannelId})")]
public class GuildChannel
{
    [StringLength(30)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string ChannelId { get; set; } = null!;

    [StringLength(30)]
    public string GuildId { get; set; } = null!;

    [ForeignKey(nameof(GuildId))]
    public Guild? Guild { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    public ChannelType ChannelType { get; set; }

    [StringLength(30)]
    public string? ParentChannelId { get; set; }

    [ForeignKey(nameof(ParentChannelId))]
    public GuildChannel? ParentChannel { get; set; }

    public long Flags { get; set; }

    public ISet<SearchItem> SearchItems { get; set; }
    public ISet<GuildUserChannel> Users { get; set; }

    public GuildChannel()
    {
        SearchItems = new HashSet<SearchItem>();
        Users = new HashSet<GuildUserChannel>();
    }

    public static GuildChannel FromDiscord(IGuild guild, IChannel channel, ChannelType channelType)
    {
        var guildChannel = new GuildChannel
        {
            ChannelId = channel.Id.ToString(),
            GuildId = guild.Id.ToString(),
            Name = channel.Name,
            ChannelType = channelType
        };

        if (channel is IThreadChannel { CategoryId: { } } thread)
            guildChannel.ParentChannelId = thread.CategoryId.Value.ToString();

        return guildChannel;
    }

    public bool HasFlag(ChannelFlags flags) => (Flags & (long)flags) != 0;

    public bool IsThread()
        => ChannelType is ChannelType.PublicThread or ChannelType.PrivateThread or ChannelType.NewsThread;

    public bool IsText()
        => ChannelType == ChannelType.Text;

    public bool IsVoice()
        => ChannelType == ChannelType.Voice;

    public bool IsStage()
        => ChannelType == ChannelType.Stage;

    public bool IsCategory()
        => ChannelType == ChannelType.Category;

    public void MarkDeleted(bool deleted)
    {
        if (deleted)
            Flags |= (long)ChannelFlags.Deleted;
        else
            Flags &= ~(long)ChannelFlags.Deleted;
    }

    public void Update(IGuildChannel channel)
    {
        Name = channel.Name;
        MarkDeleted(false);
    }
}
