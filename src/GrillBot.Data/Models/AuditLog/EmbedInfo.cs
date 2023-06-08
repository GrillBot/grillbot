using System.Collections.Generic;

namespace GrillBot.Data.Models.AuditLog;

public class EmbedInfo
{
    public string Title { get; set; }
    public string Type { get; set; }
    public string ImageInfo { get; set; }
    public string VideoInfo { get; set; }
    public string AuthorName { get; set; }
    public bool ContainsFooter { get; set; }
    public string ProviderName { get; set; }
    public string ThumbnailInfo { get; set; }
    public int FieldsCount { get; set; }
    public List<EmbedFieldInfo> Fields { get; set; }
}
