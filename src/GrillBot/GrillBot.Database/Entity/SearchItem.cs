using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrillBot.Database.Entity
{
    public class SearchItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(30)]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [Required]
        [StringLength(30)]
        public string GuildId { get; set; }

        [ForeignKey(nameof(GuildId))]
        public Guild Guild { get; set; }

        [Required]
        [StringLength(30)]
        public string ChannelId { get; set; }

        [ForeignKey(nameof(ChannelId))]
        public GuildChannel Channel { get; set; }

        [Required]
        [StringLength(1024)]
        public string MessageContent { get; set; }
    }
}
