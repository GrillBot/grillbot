using GrillBot.Core.Infrastructure;

namespace GrillBot.Common.Services.AuditLog.Models.Request.Search;

public class TextSearchRequest : IDictionaryObject
{
    public string? Text { get; set; } = null!;

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(Text), Text }
        };
    }
}
