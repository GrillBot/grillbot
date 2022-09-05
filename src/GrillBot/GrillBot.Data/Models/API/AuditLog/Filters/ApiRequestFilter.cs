using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using GrillBot.Common.Extensions;
using GrillBot.Common.Infrastructure;
using GrillBot.Database.Models;

namespace GrillBot.Data.Models.API.AuditLog.Filters;

public class ApiRequestFilter : IExtendedFilter, IApiObject
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
        var request = JsonConvert.DeserializeObject<ApiRequest>(item.Data, settings)!;

        if (!string.IsNullOrEmpty(ControllerName) && !request.ControllerName.Contains(ControllerName))
            return false;

        if (!string.IsNullOrEmpty(ActionName) && !request.ActionName.Contains(ActionName))
            return false;

        if (!IsDurationValid(Convert.ToInt32((request.EndAt - request.StartAt).TotalMilliseconds)))
            return false;

        if (!string.IsNullOrEmpty(PathTemplate) && !request.TemplatePath.StartsWith(PathTemplate))
            return false;

        if (!string.IsNullOrEmpty(Method) && Method != request.Method)
            return false;

        if (!string.IsNullOrEmpty(LoggedUserRole) && LoggedUserRole != request.LoggedUserRole)
            return false;

        return true;
    }

    private bool IsDurationValid(int duration)
        => Duration == null || (duration >= Duration.From && duration <= Duration.To);

    public Dictionary<string, string> SerializeForLog()
    {
        var result = new Dictionary<string, string>
        {
            { nameof(ControllerName), ControllerName },
            { nameof(ActionName), ActionName },
            { nameof(PathTemplate), PathTemplate },
            { nameof(Method), Method },
            { nameof(LoggedUserRole), LoggedUserRole }
        };

        result.AddApiObject(Duration, nameof(Duration));
        return result;
    }
}
