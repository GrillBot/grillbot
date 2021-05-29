using Discord.WebSocket;

namespace GrillBot.Data.Models.API
{
    public class User
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Discriminator { get; set; }

        public User() { }

        public User(SocketUser user)
        {
            Id = user.Id.ToString();
            Username = user.Username;
            Discriminator = user.Discriminator;
        }
    }
}
