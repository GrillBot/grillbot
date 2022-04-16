using Discord;
using Discord.WebSocket;
using GrillBot.Data.Extensions.Discord;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.API.Users;
using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Channel for emote suggestions.
        /// </summary>
        public Channel EmoteSuggestionChannel { get; set; }

        /// <summary>
        /// Maximum count of members.
        /// </summary>
        public int? MaxMembers { get; set; }

        /// <summary>
        /// Maximum online members.
        /// </summary>
        public int? MaxPresences { get; set; }

        /// <summary>
        /// Maximum members with webcam
        /// </summary>
        public int? MaxVideoChannelUsers { get; set; }

        /// <summary>
        /// Maximum bitrate
        /// </summary>
        public int MaxBitrate { get; set; }

        /// <summary>
        /// Maximum upload limit.
        /// </summary>
        public int MaxUploadLimit { get; set; }

        public Dictionary<UserStatus, int> UserStatusReport { get; set; }
        public Dictionary<ClientType, int> ClientTypeReport { get; set; }
        public GuildDatabaseReport DatabaseReport { get; set; }

        public GuildDetail() { }

        public GuildDetail(SocketGuild guild, Database.Entity.Guild dbGuild, GuildDatabaseReport databaseReport) : base(guild)
        {
            DatabaseReport = databaseReport;

            if (guild == null)
            {
                Name = dbGuild.Name;
                Id = dbGuild.Id;
                return;
            }

            CreatedAt = guild.CreatedAt;
            IconUrl = guild.IconUrl;
            Owner = new User(guild.Owner);
            PremiumTier = guild.PremiumTier;
            VanityUrl = !string.IsNullOrEmpty(guild.VanityURLCode) ? DiscordConfig.InviteUrl + guild.VanityURLCode : null;
            MaxMembers = guild.MaxMembers;
            MaxPresences = guild.MaxPresences;
            MaxVideoChannelUsers = guild.MaxVideoChannelUsers;
            MaxBitrate = guild.MaxBitrate / 1000;
            MaxUploadLimit = guild.CalculateFileUploadLimit();

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

            if (!string.IsNullOrEmpty(dbGuild.EmoteSuggestionChannelId))
            {
                var emoteSuggestionChannel = guild.GetTextChannel(Convert.ToUInt64(dbGuild.EmoteSuggestionChannelId));
                EmoteSuggestionChannel = emoteSuggestionChannel == null ? null : new Channel(emoteSuggestionChannel);
            }

            UserStatusReport = guild.Users.GroupBy(o =>
            {
                if (o.Status == UserStatus.AFK) return UserStatus.Idle;
                else if (o.Status == UserStatus.Invisible) return UserStatus.Offline;
                return o.Status;
            }).ToDictionary(o => o.Key, o => o.Count());

            ClientTypeReport = guild.Users
                .Where(o => o.Status != UserStatus.Offline && o.Status != UserStatus.Invisible)
                .SelectMany(o => o.ActiveClients)
                .GroupBy(o => o)
                .ToDictionary(o => o.Key, o => o.Count());
        }
    }
}
