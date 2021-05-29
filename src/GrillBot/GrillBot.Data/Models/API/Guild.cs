using Discord.WebSocket;

namespace GrillBot.Data.Models.API
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

        public Guild(SocketGuild guild)
        {
            Id = guild.Id.ToString();
            Name = guild.Name;
            MemberCount = guild.MemberCount;
        }
    }
}
