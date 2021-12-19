using Discord;
using Discord.WebSocket;
using GrillBot.Data.Extensions.Discord;
using GrillBot.Data.Helpers;
using System;
using System.Linq;

namespace GrillBot.Data.Models.API.Channels
{
    public class Channel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ChannelType? Type { get; set; }
        public int CachedMessagesCount { get; set; }

        public DateTime? FirstMessageAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public long MessagesCount { get; set; }

        public Channel() { }

        public Channel(SocketGuildChannel channel)
        {
            Id = channel.Id.ToString();
            Name = channel.Name;
            Type = DiscordHelper.GetChannelType(channel);

            var category = channel.GetCategory();
            if (category != null)
                Name += $" ({category.Name})";

            if (channel is SocketTextChannel textChannel)
                CachedMessagesCount = textChannel.CachedMessages.Count;
        }

        public Channel(Database.Entity.GuildChannel entity, int cachedMessagesCount = 0)
        {
            Id = entity.ChannelId;
            Name = entity.Name;
            Type = entity.ChannelType;
            CachedMessagesCount = cachedMessagesCount;

            if (entity.Channels.Count > 0)
            {
                FirstMessageAt = entity.Channels.Min(o => o.FirstMessageAt);
                LastMessageAt = entity.Channels.Max(o => o.LastMessageAt);
                MessagesCount = entity.Channels.Sum(o => o.Count);
            }
        }
    }
}
