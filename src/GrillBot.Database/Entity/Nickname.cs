using System.ComponentModel.DataAnnotations;

namespace GrillBot.Database.Entity;

public class Nickname
{
    [StringLength(30)]
    public string GuildId { get; set; } = null!;

    [StringLength(30)]
    public string UserId { get; set; } = null!;
    public long Id { get; set; }

    [StringLength(32)]
    [MinLength(2)]
    public string NicknameValue { get; set; } = null!;

    public GuildUser? User { get; set; }
}
