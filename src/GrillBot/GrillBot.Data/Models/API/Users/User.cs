using Discord;
using GrillBot.Database.Enums;

namespace GrillBot.Data.Models.API.Users
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

        /// <summary>
        /// Flag that describe user is bot.
        /// </summary>
        public bool IsBot { get; set; }

        /// <summary>
        /// Avatar url.
        /// </summary>
        public string AvatarUrl { get; set; }

        public User() { }

        public User(ulong id, string username, string discriminator, bool isBot, string avatarUrl)
        {
            Id = id.ToString();
            Username = username;
            Discriminator = discriminator;
            IsBot = isBot;
            AvatarUrl = avatarUrl;
        }

        public User(IUser user) : this(user.Id, user.Username, user.Discriminator, user.IsBot, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
        {
        }

        public User(Database.Entity.User user)
        {
            Id = user.Id;
            Username = user.Username;
            IsBot = user.HaveFlags(UserFlags.NotUser);
        }
    }
}
