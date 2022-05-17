using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using Newtonsoft.Json;
using System;

namespace GrillBot.Data.Models.API.AuditLog.Filters;

public class ApiRequestFilter : IExtendedFilter
{
    public string ControllerName { get; set; }
    public string ActionName { get; set; }
    public string PathTemplate { get; set; }
    public RangeParams<int> Duration { get; set; }
    public string Method { get; set; }
    public string LoggedUserRole { get; set; }

    public bool IsSet()
    {
        return !string.IsNullOrEmpty(ControllerName) || !string.IsNullOrEmpty(ActionName)
            || !string.IsNullOrEmpty(PathTemplate) || Duration != null || !string.IsNullOrEmpty(Method)
            || !string.IsNullOrEmpty(LoggedUserRole);
    }

    public bool IsValid(AuditLogItem item, JsonSerializerSettings settings)
    {
        var request = JsonConvert.DeserializeObject<ApiRequest>(item.Data, settings);

        if (!string.IsNullOrEmpty(ControllerName) && ControllerName != request.ControllerName)
            return false;

        if (!string.IsNullOrEmpty(ActionName) && ActionName != request.ActionName)
            return false;

        if (!IsDurationValid(Convert.ToInt32((request.EndAt - request.StartAt).TotalMilliseconds)))
            return false;

        if (!string.IsNullOrEmpty(PathTemplate) && !request.TemplatePath.StartsWith(PathTemplate))
            return false;

        if (!string.IsNullOrEmpty(Method) && Method != request.Method)
            return false;

        if (!string.IsNullOrEmpty(LoggedUserRole) && !string.IsNullOrEmpty(request.LoggedUserRole) && LoggedUserRole != request.LoggedUserRole)
            return false;

        return true;
    }

    private bool IsDurationValid(int duration)
        => Duration == null || (duration >= Duration.From && duration <= Duration.To);
}
