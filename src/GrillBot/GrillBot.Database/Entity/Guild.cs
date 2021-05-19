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

        public Guild()
        {
            Users = new HashSet<GuildUser>();
            Invites = new HashSet<Invite>();
            Channels = new HashSet<GuildChannel>();
        }
    }
}
