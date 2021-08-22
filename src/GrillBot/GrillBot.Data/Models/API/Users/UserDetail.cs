using Discord;
using GrillBot.Data.Models.API.Emotes;
using System.Collections.Generic;
using System.Linq;

namespace GrillBot.Data.Models.API.Users
{
    public class UserDetail
    {
        public string Id { get; set; }
        public bool HaveApiToken { get; set; }
        public string Username { get; set; }
        public string Note { get; set; }
        public long Flags { get; set; }
        public bool HaveBirthday { get; set; }
        public List<GuildUserDetail> Guilds { get; set; }
        public List<EmoteStatItem> Emotes { get; set; }
        public UserStatus Status { get; set; }
        public List<string> ActiveClients { get; set; }
        public bool IsKnown { get; set; }

        public UserDetail() { }

        public UserDetail(Database.Entity.User entity, IUser user)
        {
            Id = entity.Id;
            HaveApiToken = entity.ApiToken != null;
            Username = entity.Username;
            Note = entity.Note;
            Flags = entity.Flags;
            HaveBirthday = entity.Birthday != null;
            Guilds = entity.Guilds.Select(o => new GuildUserDetail(o)).OrderBy(o => o.Guild.Name).ToList();
            Emotes = entity.UsedEmotes.Select(o => new EmoteStatItem(o)).OrderBy(o => o.Name).ToList();
            IsKnown = user != null;

            if (IsKnown)
            {
                ActiveClients = user.ActiveClients.Select(o => o.ToString()).OrderBy(o => o).ToList();
                Status = user.Status;
            }
        }
    }
}
