using GrillBot.Database.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrillBot.Database.Entity
{
    public class User
    {
        [Key]
        [StringLength(30)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        public Guid ApiToken { get; set; }

        [Required]
        public int Flags { get; set; } = 0;

        public DateTime? Birthday { get; set; }

        [NotMapped]
        public bool BirthdayAcceptYear => Birthday != null && Birthday.Value.Year != 1;

        [Required]
        public int WebAdminLoginCount { get; set; } = 0;

        public DateTime? WebAdminBannedTo { get; set; }

        public ISet<GuildUser> Guilds { get; set; }
        public ISet<EmoteStatisticItem> UsedEmotes { get; set; }
        public ISet<RemindMessage> IncomingReminders { get; set; }
        public ISet<RemindMessage> OutgoingReminders { get; set; }
        public ISet<GuildChannel> Channels { get; set; }
        public ISet<SearchItem> SearchItems { get; set; }

        public User()
        {
            Guilds = new HashSet<GuildUser>();
            UsedEmotes = new HashSet<EmoteStatisticItem>();
            IncomingReminders = new HashSet<RemindMessage>();
            OutgoingReminders = new HashSet<RemindMessage>();
            Channels = new HashSet<GuildChannel>();
            SearchItems = new HashSet<SearchItem>();
        }

        public bool HaveFlags(UserFlags flags) => (Flags & (int)flags) != 0;
    }
}
