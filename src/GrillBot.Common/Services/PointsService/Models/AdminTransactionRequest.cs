namespace GrillBot.Common.Services.PointsService.Models;

public class AdminTransactionRequest
{
    public string GuildId { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public int Amount { get; set; }
}
