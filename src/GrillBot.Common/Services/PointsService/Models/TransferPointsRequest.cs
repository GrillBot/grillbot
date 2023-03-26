namespace GrillBot.Common.Services.PointsService.Models;

public class TransferPointsRequest
{
    public string GuildId { get; set; } = null!;
    public string FromUserId { get; set; } = null!;
    public string ToUserId { get; set; } = null!;
    public int Amount { get; set; }
}
