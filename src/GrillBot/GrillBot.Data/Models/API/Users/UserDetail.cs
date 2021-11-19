using Discord;
using GrillBot.Data.Extensions.Discord;
using GrillBot.Data.Models.API.Emotes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrillBot.Data.Models.API.Users
{
    public class UserDetail
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Note { get; set; }
        public long Flags { get; set; }
        public bool HaveBirthday { get; set; }
        public List<GuildUserDetail> Guilds { get; set; }
        public List<EmoteStatItem> Emotes { get; set; }
        public UserStatus Status { get; set; }
        public List<string> ActiveClients { get; set; }
        public bool IsKnown { get; set; }
        public string AvatarUrl { get; set; }
        public TimeSpan? SelfUnverifyMinimalTime { get; set; }

        public UserDetail() { }

        public UserDetail(Database.Entity.User entity, IUser user, IDiscordClient discordClient)
        {
            Id = entity.Id;
            Username = entity.Username;
            Note = entity.Note;
            Flags = entity.Flags;
            HaveBirthday = entity.Birthday != null;
            Guilds = entity.Guilds.Select(o => new GuildUserDetail(o, discordClient.GetGuildAsync(Convert.ToUInt64(o.GuildId)).Result)).OrderBy(o => o.Guild.Name).ToList();
            IsKnown = user != null;
            SelfUnverifyMinimalTime = entity.SelfUnverifyMinimalTime;

            Emotes = entity.UsedEmotes
                .Select(o => new EmoteStatItem(o))
                .OrderByDescending(o => o.UseCount)
                .ThenByDescending(o => o.LastOccurence)
                .ThenBy(o => o.Name)
                .ToList();

            if (IsKnown)
            {
                ActiveClients = user.ActiveClients.Select(o => o.ToString()).OrderBy(o => o).ToList();
                Status = user.Status;
                AvatarUrl = user.GetAvatarUri();
            }
            else
            {
                AvatarUrl = CDN.GetDefaultUserAvatarUrl(0);
            }
        }

        public void RemoveSecretData()
        {
            Note = null;
        }
    }
}
