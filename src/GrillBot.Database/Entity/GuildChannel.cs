using Discord;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using GrillBot.Core.Extensions;
using System;

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

    public int UserPermissionsCount { get; set; }
    public int RolePermissionsCount { get; set; }

    public int PinCount { get; set; }

    public GuildChannel()
    {
        SearchItems = new HashSet<SearchItem>();
        Users = new HashSet<GuildUserChannel>();
    }

    public static GuildChannel FromDiscord(IGuildChannel channel, ChannelType channelType)
    {
        var guildChannel = new GuildChannel
        {
            ChannelId = channel.Id.ToString(),
            GuildId = channel.Guild.Id.ToString(),
            ChannelType = channelType
        };

        if (channel is IThreadChannel { CategoryId: not null } thread)
            guildChannel.ParentChannelId = thread.CategoryId.Value.ToString();

        guildChannel.Update(channel);
        return guildChannel;
    }

    public bool HasFlag(Enums.ChannelFlag flag) => (Flags & (long)flag) != 0;

    public bool IsThread()
        => ChannelType is ChannelType.PublicThread or ChannelType.PrivateThread or ChannelType.NewsThread;

    public bool IsText()
        => ChannelType == ChannelType.Text;

    public bool IsVoice()
        => ChannelType == ChannelType.Voice;

    public bool IsCategory()
        => ChannelType == ChannelType.Category;

    public void MarkDeleted(bool deleted)
    {
        Flags = Flags.UpdateFlags((long)Enums.ChannelFlag.Deleted, deleted);
    }

    public void Update(IGuildChannel channel)
    {
        Name = channel.Name;
        MarkDeleted(channel is IThreadChannel { IsArchived: true });

        if (IsThread() || channel is IThreadChannel || channel.PermissionOverwrites == null)
            return;

        RolePermissionsCount = channel.PermissionOverwrites.Count(o => o.TargetType == PermissionTarget.Role && o.TargetId != channel.Guild.EveryoneRole.Id);
        UserPermissionsCount = channel.PermissionOverwrites.Count(o => o.TargetType == PermissionTarget.User);
    }

    public GuildChannel Clone()
    {
        return new GuildChannel
        {
            Flags = Flags,
            ChannelType = ChannelType,
            Guild = Guild,
            GuildId = GuildId,
            Name = Name,
            ChannelId = ChannelId,
            ParentChannel = ParentChannel?.Clone(),
            SearchItems = SearchItems,
            ParentChannelId = ParentChannelId,
            RolePermissionsCount = RolePermissionsCount,
            UserPermissionsCount = UserPermissionsCount,
            Users = Users,
            PinCount = PinCount
        };
    }

    public string GetHyperlink()
        => $"[#{Name}](https://discord.com/channels/{GuildId}/{ChannelId}/{SnowflakeUtils.ToSnowflake(DateTimeOffset.Now)})";
}
