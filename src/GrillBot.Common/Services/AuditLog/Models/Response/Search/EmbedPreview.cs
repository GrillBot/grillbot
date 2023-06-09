namespace GrillBot.Common.Services.AuditLog.Models.Response.Search;

public class EmbedPreview
{
    public string? Title { get; set; }
    public string Type { get; set; } = null!;
    public int FieldsCount { get; set; }
    public bool ContainsFooter { get; set; }
    public string? ProviderName { get; set; }
    public string? AuthorName { get; set; }
    public string? ImageInfo { get; set; }
    public string? VideoInfo { get; set; }
    public string? ThumbnailInfo { get; set; }
}
