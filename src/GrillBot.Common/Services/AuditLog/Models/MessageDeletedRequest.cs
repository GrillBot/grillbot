using System.ComponentModel.DataAnnotations;

namespace GrillBot.Common.Services.AuditLog.Models;

public class MessageDeletedRequest
{
    public string AuthorId { get; set; } = null!;
    public DateTime MessageCreatedAt { get; set; }
    public string? Content { get; set; }
    public List<EmbedRequest> Embeds { get; set; } = new();
}
