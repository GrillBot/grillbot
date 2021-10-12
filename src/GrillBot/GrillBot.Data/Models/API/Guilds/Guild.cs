using Discord;
using Discord.WebSocket;

namespace GrillBot.Data.Models.API.Guilds
{
    /// <summary>
    /// Simple guild item.
    /// </summary>
    public class Guild
    {
        /// <summary>
        /// Guild ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of guild.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Member count.
        /// </summary>
        public int MemberCount { get; set; }

        /// <summary>
        /// Flag that describe information about connection state.
        /// </summary>
        public bool IsConnected { get; set; }

        public Guild() { }

        public Guild(Database.Entity.Guild guild)
        {
            Id = guild.Id;
            Name = guild.Name;
        }

        public Guild(SocketGuild guild)
        {
            if (guild == null) return;

            Id = guild.Id.ToString();
            Name = guild.Name;
            MemberCount = guild.MemberCount;
            IsConnected = guild.IsConnected;
        }

        public Guild(IGuild guild)
        {
            if (guild == null) return;

            Id = guild.Id.ToString();
            Name = guild.Name;
            MemberCount = guild.ApproximateMemberCount ?? 0;
        }
    }
}
