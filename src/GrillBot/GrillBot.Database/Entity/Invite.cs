using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrillBot.Database.Entity
{
    public class Invite
    {
        [Key]
        [StringLength(10)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Code { get; set; }

        public DateTime? CreatedAt { get; set; }

        [StringLength(30)]
        public string CreatorId { get; set; }

        [StringLength(30)]
        public string GuildId { get; set; }

        public GuildUser Creator { get; set; }

        public ISet<GuildUser> UsedUsers { get; set; }

        public Invite()
        {
            UsedUsers = new HashSet<GuildUser>();
        }
    }
}
