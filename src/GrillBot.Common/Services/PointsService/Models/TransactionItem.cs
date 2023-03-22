namespace GrillBot.Common.Services.PointsService.Models;

public class TransactionItem
{
    public string GuildId { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string MessageId { get; set; } = null!;
    public bool IsReaction { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Value { get; set; }
    public int MergedCount { get; set; }
    public DateTime? MergedFrom { get; set; }
    public DateTime? MergedTo { get; set; }
}
