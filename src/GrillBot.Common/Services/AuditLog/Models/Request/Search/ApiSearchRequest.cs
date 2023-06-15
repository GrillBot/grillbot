using GrillBot.Core.Infrastructure;

namespace GrillBot.Common.Services.AuditLog.Models.Request.Search;

public class ApiSearchRequest : IDictionaryObject
{
    public string? ControllerName { get; set; }
    public string? ActionName { get; set; }
    public string? PathTemplate { get; set; }
    public int? DurationFrom { get; set; }
    public int? DurationTo { get; set; }
    public string? Method { get; set; }
    public string? ApiGroupName { get; set; }

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(ControllerName), ControllerName },
            { nameof(ActionName), ActionName },
            { nameof(PathTemplate), PathTemplate },
            { nameof(DurationFrom), DurationFrom?.ToString() },
            { nameof(DurationTo), DurationTo?.ToString() },
            { nameof(Method), Method },
            { nameof(ApiGroupName), ApiGroupName }
        };
    }
}
