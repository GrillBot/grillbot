using GrillBot.Core.Infrastructure;

namespace GrillBot.Common.Services.AuditLog.Models.Request.Search;

public class MessageDeletedSearchRequest : IDictionaryObject
{
    public bool? ContainsEmbed { get; set; }
    public string? ContentContains { get; set; }
    public string? AuthorId { get; set; }

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(ContainsEmbed), ContainsEmbed?.ToString() },
            { nameof(ContentContains), ContentContains },
            { nameof(AuthorId), AuthorId }
        };
    }
}
