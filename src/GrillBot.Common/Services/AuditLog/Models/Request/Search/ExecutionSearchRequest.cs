using GrillBot.Core.Infrastructure;

namespace GrillBot.Common.Services.AuditLog.Models.Request.Search;

public class ExecutionSearchRequest : IDictionaryObject
{
    public string? ActionName { get; set; }
    public bool? Success { get; set; }
    public int? DurationFrom { get; set; }
    public int? DurationTo { get; set; }

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(ActionName), ActionName },
            { nameof(Success), Success?.ToString() },
            { nameof(DurationFrom), DurationFrom?.ToString() },
            { nameof(DurationTo), DurationTo?.ToString() }
        };
    }
}
