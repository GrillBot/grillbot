using GrillBot.Core.Infrastructure;

namespace GrillBot.Common.Services.AuditLog.Models.Request.Search;

public class UserIdSearchRequest : IDictionaryObject
{
    public string? UserId { get; set; }

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(UserId), UserId }
        };
    }
}
