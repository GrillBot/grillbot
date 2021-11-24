using Discord.WebSocket;
using GrillBot.Data.Models.API.Guilds;

namespace GrillBot.Data.Models.API.Channels
{
    public class GuildChannel : Channel
    {
        public Guild Guild { get; set; }

        public GuildChannel() { }

        public GuildChannel(Database.Entity.GuildChannel channel, int cachedMessagesCount = 0) : base(channel, cachedMessagesCount)
        {
            Guild = channel.Guild == null ? null : new(channel.Guild);
        }

        public GuildChannel(SocketGuildChannel channel) : base(channel)
        {
            Guild = channel.Guild == null ? null : new(channel.Guild);
        }
    }
}
