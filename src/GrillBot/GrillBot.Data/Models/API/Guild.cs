using Discord.WebSocket;

namespace GrillBot.Data.Models.API
{
    public class Guild
    {
        public string Id { get; set; }
        public string Name { get; set; }
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
