namespace GrillBot.Common.Services.ImageProcessing.Models;

public class PointsRequest
{
    public string UserId { get; set; } = null!;
    public string Username { get; set; } = null!;
    public int PointsValue { get; set; }
    public int Position { get; set; }
    public AvatarInfo AvatarInfo { get; set; } = null!;
}
