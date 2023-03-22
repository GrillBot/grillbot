using System.Collections.Generic;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Newtonsoft.Json;

namespace GrillBot.Data.Models.API.AuditLog.Filters;

public class TargetIdFilter : IExtendedFilter, IDictionaryObject
{
    public string? TargetId { get; set; }

    public bool IsSet()
        => !string.IsNullOrEmpty(TargetId);

    public bool IsValid(AuditLogItem item, JsonSerializerSettings settings)
    {
        switch (item.Type)
        {
            case AuditLogItemType.MemberUpdated or AuditLogItemType.MemberRoleUpdated:
                return IsValidMemberTarget(JsonConvert.DeserializeObject<MemberUpdatedData>(item.Data, settings)!);
            case AuditLogItemType.OverwriteCreated or AuditLogItemType.OverwriteDeleted:
                return IsValidOverwrite(JsonConvert.DeserializeObject<AuditOverwriteInfo>(item.Data, settings)!);
            case AuditLogItemType.OverwriteUpdated:
                var diff = JsonConvert.DeserializeObject<Diff<AuditOverwriteInfo>>(item.Data, settings);
                return IsValidOverwrite(diff!.Before) || IsValidOverwrite(diff.After);
            default:
                return false;
        }
    }

    private bool IsValidMemberTarget(MemberUpdatedData data)
        => (!string.IsNullOrEmpty(data.Target.UserId) ? data.Target.UserId : data.Target.Id.ToString()) == TargetId;

    private bool IsValidOverwrite(AuditOverwriteInfo overwriteInfo)
        => (!string.IsNullOrEmpty(overwriteInfo.TargetIdValue) ? overwriteInfo.TargetIdValue : overwriteInfo.TargetId.ToString()) == TargetId;

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(TargetId), TargetId }
        };
    }
}
