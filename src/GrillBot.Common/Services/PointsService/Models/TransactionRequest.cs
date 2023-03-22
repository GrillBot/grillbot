namespace GrillBot.Common.Services.PointsService.Models;

public class TransactionRequest
{
    public string GuildId { get; set; } = null!;
    public string ChannelId { get; set; } = null!;
    public MessageInfo MessageInfo { get; set; } = null!;
    public ReactionInfo? ReactionInfo { get; set; }
}
