using Discord.WebSocket;
using System;

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
        }
    }
}
