using Discord;
using Discord.WebSocket;
using GrillBot.Data.Models.API.Guilds;
using System;
using System.Linq;

namespace GrillBot.Data.Models.API.Channels;

public class GuildChannelListItem : Channel
{
    public Guild Guild { get; set; }

    public DateTime? FirstMessageAt { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public long MessagesCount { get; set; }
    public int CachedMessagesCount { get; set; }

    public int? RolePermissionCount { get; set; }
    public int? UserPermissionCount { get; set; }

    public GuildChannelListItem() { }

    public GuildChannelListItem(Database.Entity.GuildChannel entity, int cachedMessagesCount = 0, SocketGuildChannel guildChannel = null) : base(entity)
    {
        Guild = entity.Guild == null ? null : new(entity.Guild);

        CachedMessagesCount = cachedMessagesCount;
        if (entity.Users.Count > 0)
        {
            FirstMessageAt = entity.Users.Min(o => o.FirstMessageAt);
            LastMessageAt = entity.Users.Max(o => o.LastMessageAt);
            MessagesCount = entity.Users.Sum(o => o.Count);

            if (FirstMessageAt == DateTime.MinValue) FirstMessageAt = null;
            if (LastMessageAt == DateTime.MinValue) LastMessageAt = null;
        }

        if (guildChannel != null && guildChannel is not SocketThreadChannel)
        {
            var overwrites = guildChannel.PermissionOverwrites?
                .Where(o => o.TargetId != guildChannel.Guild.EveryoneRole.Id)
                .GroupBy(o => o.TargetType)
                .ToDictionary(o => o.Key, o => o.Count());

            int val;
            RolePermissionCount = overwrites.TryGetValue(PermissionTarget.Role, out val) ? val : 0;
            UserPermissionCount = overwrites.TryGetValue(PermissionTarget.User, out val) ? val : 0;
        }
    }

    public GuildChannelListItem(SocketGuildChannel channel) : base(channel)
    {
        Guild = channel.Guild == null ? null : new(channel.Guild);

        if (channel is SocketTextChannel textChannel)
            CachedMessagesCount = textChannel.CachedMessages.Count;
    }
}
