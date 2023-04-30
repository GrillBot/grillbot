namespace GrillBot.Common.Services.ImageProcessing.Models;

public class PeepoRequest
{
    public long GuildUploadLimit { get; set; }
    public string UserId { get; set; } = null!;
    public AvatarInfo AvatarInfo { get; set; } = null!;
}
