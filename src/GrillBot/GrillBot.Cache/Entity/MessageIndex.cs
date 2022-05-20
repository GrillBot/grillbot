using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrillBot.Cache.Entity;

[Index(nameof(ChannelId), Name = "IX_MessageCache_ChannelId")]
[Index(nameof(AuthorId), Name = "IX_MessageCache_AuthorId")]
[Index(nameof(GuildId), Name = "IX_MessageCache_GuildId")]
public class MessageIndex
{
    [Key]
    [StringLength(30)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string MessageId { get; set; } = null!;

    [Required]
    [StringLength(30)]
    public string ChannelId { get; set; } = null!;

    [Required]
    [StringLength(30)]
    public string AuthorId { get; set; } = null!;

    [Required]
    [StringLength(30)]
    public string GuildId { get; set; } = null!;
}
