using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrillBot.Database.Entity
{
    public class GuildUser
    {
        [StringLength(30)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [StringLength(30)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string GuildId { get; set; }

        [ForeignKey(nameof(GuildId))]
        public Guild Guild { get; set; }

        [StringLength(20)]
        public string UsedInviteCode { get; set; }

        [ForeignKey(nameof(UsedInviteCode))]
        public Invite UsedInvite { get; set; }

        [Required]
        public long Points { get; set; } = 0;

        public DateTime? LastPointsReactionIncrement { get; set; }
        public DateTime? LastPointsMessageIncrement { get; set; }

        [Required]
        public long GivenReactions { get; set; } = 0;

        [Required]
        public long ObtainedReactions { get; set; } = 0;

        public ISet<Invite> CreatedInvites { get; set; }
        public Unverify Unverify { get; set; }

        [StringLength(32)]
        [MinLength(2)]
        public string Nickname { get; set; }

        public GuildUser()
        {
            CreatedInvites = new HashSet<Invite>();
        }
    }
}
