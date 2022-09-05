using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using GrillBot.Common.Extensions;
using GrillBot.Common.Infrastructure;
using GrillBot.Database.Models;

namespace GrillBot.Data.Models.API.AuditLog.Filters;

public class ExecutionFilter : IExtendedFilter, IApiObject
{
    public string Name { get; set; }
    public bool? WasSuccess { get; set; }
    public RangeParams<int> Duration { get; set; }

    public bool IsSet()
    {
        return !string.IsNullOrEmpty(Name) || WasSuccess != null || Duration != null;
    }

    public bool IsValid(AuditLogItem item, JsonSerializerSettings settings)
    {
        return item.Type switch
        {
            AuditLogItemType.Command => IsValidCommand(JsonConvert.DeserializeObject<CommandExecution>(item.Data, settings)),
            AuditLogItemType.InteractionCommand => IsValidInteraction(JsonConvert.DeserializeObject<InteractionCommandExecuted>(item.Data, settings)),
            AuditLogItemType.JobCompleted => IsValidJob(JsonConvert.DeserializeObject<JobExecutionData>(item.Data, settings)),
            _ => false,
        };
    }

    public bool IsValidCommand(CommandExecution data)
    {
        if (!string.IsNullOrEmpty(Name) && !data.Command.Contains(Name))
            return false;

        if (WasSuccess != null && data.IsSuccess != WasSuccess.Value)
            return false;

        return IsDurationValid(data.Duration);
    }

    public bool IsValidInteraction(InteractionCommandExecuted data)
    {
        if (!string.IsNullOrEmpty(Name) && !data.FullName.Contains(Name))
            return false;

        if (WasSuccess != null && data.IsSuccess != WasSuccess.Value)
            return false;

        return IsDurationValid(data.Duration);
    }

    public bool IsValidJob(JobExecutionData data)
    {
        if (!string.IsNullOrEmpty(Name) && !data.JobName.Contains(Name))
            return false;

        if (WasSuccess != null && (!data.WasError) != WasSuccess.Value)
            return false;

        return IsDurationValid(Convert.ToInt32((data.EndAt - data.StartAt).TotalMilliseconds));
    }

    private bool IsDurationValid(int duration)
        => Duration == null || (duration >= Duration.From && duration <= Duration.To);

    public Dictionary<string, string> SerializeForLog()
    {
        var result = new Dictionary<string, string>
        {
            { nameof(Name), Name },
            { nameof(WasSuccess), WasSuccess?.ToString() },
        };

        result.AddApiObject(Duration, nameof(Duration));
        return result;
    }
}
