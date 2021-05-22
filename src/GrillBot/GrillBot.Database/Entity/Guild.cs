using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrillBot.Database.Entity
{
    public class Guild
    {
        [Key]
        [StringLength(30)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        public ISet<GuildUser> Users { get; set; }
        public ISet<Invite> Invites { get; set; }
        public ISet<GuildChannel> Channels { get; set; }
        public ISet<SearchItem> Searches { get; set; }
        public ISet<Unverify> Unverifies { get; set; }
        public ISet<UnverifyLog> UnverifyLogs { get; set; }

        public Guild()
        {
            Users = new HashSet<GuildUser>();
            Invites = new HashSet<Invite>();
            Channels = new HashSet<GuildChannel>();
            Unverifies = new HashSet<Unverify>();
            UnverifyLogs = new HashSet<UnverifyLog>();
            Searches = new HashSet<SearchItem>();
        }
    }
}
