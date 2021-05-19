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
        public string GuildChannelId { get; set; }

        [ForeignKey(nameof(GuildChannelId))]
        public GuildChannel Channel { get; set; }

        [Required]
        [StringLength(30)]
        public string MessageId { get; set; }
    }
}
