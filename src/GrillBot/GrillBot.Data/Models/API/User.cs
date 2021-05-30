using Discord;

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

        public bool IsBot { get; set; }

        public User() { }

        public User(ulong id, string username, string discriminator, bool isBot)
        {
            Id = id.ToString();
            Username = username;
            Discriminator = discriminator;
            IsBot = isBot;
        }

        public User(IUser user) : this(user.Id, user.Username, user.Discriminator, user.IsBot)
        {
        }
    }
}
