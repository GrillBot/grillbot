using Discord.WebSocket;

namespace GrillBot.Data.Models.API
{
    public class User
    {
        /// <summary>
        /// Discord ID of user.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Username.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// User hash.
        /// </summary>
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
