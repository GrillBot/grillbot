using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.AuditLog;
using System;

namespace GrillBot.Data.Models.API.AuditLog.Filters;

public class ExecutionFilter
{
    public string Name { get; set; }
    public bool? WasSuccess { get; set; }
    public RangeParams<int> Duration { get; set; }

    public bool IsValidCommand(CommandExecution data)
    {
        if (!string.IsNullOrEmpty(Name) && !data.Command.Contains(Name))
            return false;

        if (WasSuccess != null && data.IsSuccess != WasSuccess.Value)
            return false;

        if (!IsDurationValid(data.Duration))
            return false;

        return true;
    }

    public bool IsValidInteraction(InteractionCommandExecuted data)
    {
        if (!string.IsNullOrEmpty(Name) && !data.FullName.Contains(Name))
            return false;

        if (WasSuccess != null && data.IsSuccess != WasSuccess.Value)
            return false;

        if (!IsDurationValid(data.Duration))
            return false;

        return true;
    }

    public bool IsValidJob(JobExecutionData data)
    {
        if (!string.IsNullOrEmpty(Name) && !data.JobName.Contains(Name))
            return false;

        if (WasSuccess != null && (!data.WasError) != WasSuccess.Value)
            return false;

        if (!IsDurationValid(Convert.ToInt32((data.EndAt - data.StartAt).TotalMilliseconds)))
            return false;

        return true;
    }

    private bool IsDurationValid(int duration)
        => Duration == null || (Duration.From >= duration && Duration.To <= duration);
}
