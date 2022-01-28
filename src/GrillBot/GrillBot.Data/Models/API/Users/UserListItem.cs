using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrillBot.Data.Models.API.Users
{
    public class UserListItem
    {
        public string Id { get; set; }
        public int Flags { get; set; }
        public bool HaveBirthday { get; set; }
        public string Username { get; set; }
        public UserStatus DiscordStatus { get; set; }
        public DateTime? RegisteredAt { get; set; }

        /// <summary>
        /// Guild names where user is/was.
        /// </summary>
        public Dictionary<string, bool> Guilds { get; set; }

        public UserListItem() { }

        public UserListItem(Database.Entity.User user, DiscordSocketClient discordClient, IUser dcUser)
        {
            Id = user.Id;
            HaveBirthday = user.Birthday != null;
            Flags = user.Flags;
            Username = string.IsNullOrEmpty(user.Discriminator) ? user.Username : $"{user.Username}#{user.Discriminator}";
            DiscordStatus = dcUser?.Status ?? UserStatus.Offline;

            if (dcUser != null)
                RegisteredAt = dcUser.CreatedAt.LocalDateTime;

            Guilds = user.Guilds.OrderBy(o => o.Guild.Name).ToDictionary(
                o => o.Guild.Name,
                o => discordClient.GetGuild(Convert.ToUInt64(o.GuildId))?.GetUser(Convert.ToUInt64(o.UserId)) != null
            );
        }
    }
}
