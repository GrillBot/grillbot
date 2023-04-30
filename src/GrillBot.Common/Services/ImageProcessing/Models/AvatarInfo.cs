namespace GrillBot.Common.Services.ImageProcessing.Models;

public class AvatarInfo
{
    public string AvatarId { get; set; } = null!;
    public byte[] AvatarContent { get; set; } = null!;
    public string Type { get; set; } = null!;
}
