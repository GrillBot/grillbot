namespace GrillBot.Common.Services.AuditLog.Models.Response.Detail;

public class MessageDeletedDetail
{
    public string AuthorId { get; set; } = null!;
    public DateTime MessageCreatedAt { get; set; }
    public string? Content { get; set; }
    public List<EmbedDetail> Embeds { get; set; } = new();
}
