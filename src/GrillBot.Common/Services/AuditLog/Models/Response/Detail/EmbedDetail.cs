namespace GrillBot.Common.Services.AuditLog.Models.Response.Detail;

public class EmbedDetail
{
    public string? Title { get; set; }
    public string Type { get; set; } = null!;
    public string? ImageInfo { get; set; }
    public string? VideoInfo { get; set; }
    public string? AuthorName { get; set; }
    public bool ContainsFooter { get; set; }
    public string? ProviderName { get; set; }
    public string? ThumbnailInfo { get; set; }
    public List<EmbedField> Fields { get; set; } = null!;
}
