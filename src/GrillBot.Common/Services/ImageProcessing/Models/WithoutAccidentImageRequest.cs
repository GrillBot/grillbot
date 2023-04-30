namespace GrillBot.Common.Services.ImageProcessing.Models;

public class WithoutAccidentImageRequest
{
    public string UserId { get; set; } = null!;
    public int DaysCount { get; set; }
    public AvatarInfo AvatarInfo { get; set; } = null!;
}
