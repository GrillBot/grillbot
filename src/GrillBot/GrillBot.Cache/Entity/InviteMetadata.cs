using System.ComponentModel.DataAnnotations;

namespace GrillBot.Cache.Entity;

public class InviteMetadata
{
    [StringLength(30)]
    public string GuildId { get; set; } = null!;

    [StringLength(10)]
    public string Code { get; set; } = null!;
    
    [Required]
    public int Uses { get; set; }
    
    [Required]
    public bool IsVanity { get; set; }

    [StringLength(30)]
    public string? CreatorId { get; set; }
    
    public DateTime? CreatedAt { get; set; }
}
