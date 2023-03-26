namespace GrillBot.Common.Services.PointsService.Models;

public class ReactionInfo
{
    public string UserId { get; set; } = null!;
    public string Emote { get; set; } = null!;

    public string GetReactionId()
        => $"{UserId}_{Emote}";
}
