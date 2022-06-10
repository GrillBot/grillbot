using System.ComponentModel.DataAnnotations;

namespace GrillBot.Cache.Entity;

public class ProfilePicture
{
    [Required]
    [StringLength(30)]
    public string UserId { get; set; } = null!;

    [Required]
    public short Size { get; set; }

    [Required]
    [StringLength(255)]
    public string AvatarId { get; set; } = null!;

    [Required]
    public bool IsAnimated { get; set; }

    [Required]
    public byte[] Data { get; set; } = null!;
}
