namespace GrillBot.Common.Services.Graphics.Models.Images;

public class PointsImageRequest
{
    public long Points { get; set; }
    public int Position { get; set; }
    public string Nickname { get; set; } = null!;
    public string ProfilePicture { get; set; } = null!;
    public string BackgroundColor { get; set; } = null!;
    public string TextBackground { get; set; } = null!;
}
