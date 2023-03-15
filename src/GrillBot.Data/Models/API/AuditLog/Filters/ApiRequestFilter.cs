using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using GrillBot.Common.Extensions;
using GrillBot.Core.Infrastructure;
using GrillBot.Database.Models;

namespace GrillBot.Data.Models.API.AuditLog.Filters;

public class ApiRequestFilter : IExtendedFilter, IDictionaryObject
{
    public string? ControllerName { get; set; }
    public string? ActionName { get; set; }
    public string? PathTemplate { get; set; }
    public RangeParams<int>? Duration { get; set; }
    public string? Method { get; set; }
    public string? LoggedUserRole { get; set; }
    public string? ApiGroupName { get; set; }

    public bool IsSet()
    {
        if (!string.IsNullOrEmpty(ControllerName)) return true;
        if (!string.IsNullOrEmpty(ActionName)) return true;
        if (!string.IsNullOrEmpty(PathTemplate)) return true;
        if (Duration != null && (Duration.From >= 0 || Duration.To >= 0)) return true;
        if (!string.IsNullOrEmpty(Method)) return true;
        if (!string.IsNullOrEmpty(LoggedUserRole)) return true;
        if (!string.IsNullOrEmpty(ApiGroupName)) return true;

        return false;
    }

    public bool IsValid(AuditLogItem item, JsonSerializerSettings settings)
    {
        var request = JsonConvert.DeserializeObject<ApiRequest>(item.Data, settings)!;

        if (!string.IsNullOrEmpty(ControllerName) && !request.ControllerName.Contains(ControllerName, StringComparison.InvariantCultureIgnoreCase)) return false;
        if (!string.IsNullOrEmpty(ActionName) && !request.ActionName.Contains(ActionName, StringComparison.InvariantCultureIgnoreCase)) return false;
        if (!IsDurationValid(request.EndAt - request.StartAt)) return false;
        if (!string.IsNullOrEmpty(PathTemplate) && !request.TemplatePath.StartsWith(PathTemplate, StringComparison.InvariantCultureIgnoreCase)) return false;
        if (!string.IsNullOrEmpty(Method) && Method != request.Method) return false;
        if (!string.IsNullOrEmpty(LoggedUserRole) && LoggedUserRole != request.LoggedUserRole) return false;
        if (!string.IsNullOrEmpty(ApiGroupName) && ApiGroupName != request.ApiGroupName) return false;

        return true;
    }

    private bool IsDurationValid(TimeSpan duration)
        => Duration == null || (duration.TotalMilliseconds >= Duration.From && duration.TotalMilliseconds <= Duration.To);

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(ControllerName), ControllerName },
            { nameof(ActionName), ActionName },
            { nameof(PathTemplate), PathTemplate },
            { nameof(Method), Method },
            { nameof(LoggedUserRole), LoggedUserRole },
            { nameof(ApiGroupName), ApiGroupName }
        };

        result.MergeDictionaryObjects(Duration, nameof(Duration));
        return result;
    }
}
