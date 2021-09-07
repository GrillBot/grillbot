using Discord;
using Discord.WebSocket;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.API.Users;
using System;

namespace GrillBot.Data.Models.API.Guilds
{
    /// <summary>
    /// Detailed information about guild.
    /// </summary>
    public class GuildDetail : Guild
    {
        /// <summary>
        /// Datetime of guild creation in UTC.
        /// </summary>
        public DateTimeOffset? CreatedAt { get; set; }

        /// <summary>
        /// Icon URL
        /// </summary>
        public string IconUrl { get; set; }

        /// <summary>
        /// Flag that describe information about connection state.
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// Owner of guild
        /// </summary>
        public User Owner { get; set; }

        /// <summary>
        /// Premium level
        /// </summary>
        public PremiumTier? PremiumTier { get; set; }

        /// <summary>
        /// Vanity invite url if exists.
        /// </summary>
        public string VanityUrl { get; set; }

        /// <summary>
        /// Muted role used to unverify.
        /// </summary>
        public Role MutedRole { get; set; }

        /// <summary>
        /// Premium user role.
        /// </summary>
        public Role BoosterRole { get; set; }

        /// <summary>
        /// Admin channel.
        /// </summary>
        public Channel AdminChannel { get; set; }

        public GuildDetail() { }

        public GuildDetail(SocketGuild guild, Database.Entity.Guild dbGuild) : base(guild)
        {
            if (guild == null)
            {
                Name = dbGuild.Name;
                Id = dbGuild.Id;
                return;
            }

            CreatedAt = guild.CreatedAt;
            IconUrl = guild.IconUrl;
            IsConnected = guild.IsConnected;
            Owner = new User(guild.Owner);
            PremiumTier = guild.PremiumTier;
            VanityUrl = !string.IsNullOrEmpty(guild.VanityURLCode) ? DiscordConfig.InviteUrl + guild.VanityURLCode : null;

            if (!string.IsNullOrEmpty(dbGuild.AdminChannelId))
            {
                var adminChannel = guild.GetChannel(Convert.ToUInt64(dbGuild.AdminChannelId));
                AdminChannel = adminChannel == null ? null : new Channel(adminChannel);
            }

            if (!string.IsNullOrEmpty(dbGuild.BoosterRoleId))
            {
                var boosterRole = guild.GetRole(Convert.ToUInt64(dbGuild.BoosterRoleId));
                BoosterRole = boosterRole == null ? null : new Role(boosterRole);
            }

            if (!string.IsNullOrEmpty(dbGuild.MuteRoleId))
            {
                var mutedRole = guild.GetRole(Convert.ToUInt64(dbGuild.MuteRoleId));
                MutedRole = mutedRole == null ? null : new Role(mutedRole);
            }
        }
    }
}
