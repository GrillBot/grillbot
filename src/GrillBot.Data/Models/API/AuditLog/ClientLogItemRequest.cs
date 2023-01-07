using GrillBot.Database.Enums;
using System.Collections.Generic;
using GrillBot.Common.Infrastructure;

namespace GrillBot.Data.Models.API.AuditLog;

public class ClientLogItemRequest : IApiObject
{
    public bool IsInfo { get; set; }
    public bool IsError { get; set; }
    public bool IsWarning { get; set; }

    public string Content { get; set; }

    public AuditLogItemType GetAuditLogType()
    {
        if (IsError) return AuditLogItemType.Error;
        return IsWarning ? AuditLogItemType.Warning : AuditLogItemType.Info;
    }

    public Dictionary<string, string> SerializeForLog()
    {
        return new Dictionary<string, string>
        {
            { nameof(IsInfo), IsInfo.ToString() },
            { nameof(IsError), IsError.ToString() },
            { nameof(IsWarning), IsWarning.ToString() },
            { nameof(Content), Content }
        };
    }
}
