using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrillBot.Database.Entity.Cache;

[Index(nameof(ChannelId), Name = "IX_MessageCache_ChannelId")]
[Index(nameof(AuthorId), Name = "IX_MessageCache_AuthorId")]
[Index(nameof(GuildId), Name = "IX_MessageCache_GuildId")]
public class MessageCacheIndex
{
    [Key]
    [StringLength(30)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string MessageId { get; set; }

    [Required]
    [StringLength(30)]
    public string ChannelId { get; set; }

    [Required]
    [StringLength(30)]
    public string AuthorId { get; set; }

    [Required]
    [StringLength(30)]
    public string GuildId { get; set; }
}
