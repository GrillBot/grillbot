namespace GrillBot.Common.Services.AuditLog.Models.Response.Search;

public class MessageDeletedPreview
{
    public string AuthorId { get; set; } = null!;
    public DateTime MessageCreatedAt { get; set; }
    public string? Content { get; set; }
    public List<EmbedPreview> Embeds { get; set; } = new();
}
