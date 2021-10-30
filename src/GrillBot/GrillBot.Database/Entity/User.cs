using Discord;
using GrillBot.Database.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace GrillBot.Database.Entity
{
    [DebuggerDisplay("{Username} ({Id})")]
    public class User
    {
        [Key]
        [StringLength(30)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        public Guid? ApiToken { get; set; }

        [Required]
        public int Flags { get; set; } = 0;

        public DateTime? Birthday { get; set; }

        [NotMapped]
        public bool BirthdayAcceptYear => Birthday != null && Birthday.Value.Year != 1;

        public string Note { get; set; }

        [StringLength(32)]
        [MinLength(2)]
        [Required]
        public string Username { get; set; }

        public TimeSpan? SelfUnverifyMinimalTime { get; set; }

        public ISet<GuildUser> Guilds { get; set; }
        public ISet<EmoteStatisticItem> UsedEmotes { get; set; }
        public ISet<RemindMessage> IncomingReminders { get; set; }
        public ISet<RemindMessage> OutgoingReminders { get; set; }
        public ISet<GuildUserChannel> Channels { get; set; }
        public ISet<SearchItem> SearchItems { get; set; }

        public User()
        {
            Guilds = new HashSet<GuildUser>();
            UsedEmotes = new HashSet<EmoteStatisticItem>();
            IncomingReminders = new HashSet<RemindMessage>();
            OutgoingReminders = new HashSet<RemindMessage>();
            Channels = new HashSet<GuildUserChannel>();
            SearchItems = new HashSet<SearchItem>();
        }

        public bool HaveFlags(UserFlags flags) => (Flags & (int)flags) != 0;

        public static User FromDiscord(IUser user)
        {
            return new User()
            {
                Id = user.Id.ToString(),
                Username = user.Username,
                Flags = (int)(user.IsBot || user.IsWebhook ? UserFlags.NotUser : UserFlags.None)
            };
        }
    }
}
