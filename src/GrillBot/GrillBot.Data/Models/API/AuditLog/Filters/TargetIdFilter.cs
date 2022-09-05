using System.Collections.Generic;
using GrillBot.Common.Infrastructure;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Newtonsoft.Json;

namespace GrillBot.Data.Models.API.AuditLog.Filters;

public class TargetIdFilter : IExtendedFilter, IApiObject
{
    public string TargetId { get; set; }

    public bool IsSet()
        => !string.IsNullOrEmpty(TargetId);

    public bool IsValid(AuditLogItem item, JsonSerializerSettings settings)
    {
        switch (item.Type)
        {
            case AuditLogItemType.MemberUpdated or AuditLogItemType.MemberRoleUpdated:
                return IsValidMemberTarget(JsonConvert.DeserializeObject<MemberUpdatedData>(item.Data, settings));
            case AuditLogItemType.OverwriteCreated or AuditLogItemType.OverwriteDeleted:
                return IsValidOverwrite(JsonConvert.DeserializeObject<AuditOverwriteInfo>(item.Data, settings));
            case AuditLogItemType.OverwriteUpdated:
                var diff = JsonConvert.DeserializeObject<Diff<AuditOverwriteInfo>>(item.Data, settings);
                return IsValidOverwrite(diff!.Before) || IsValidOverwrite(diff!.After);
            default:
                return false;
        }
    }

    private bool IsValidMemberTarget(MemberUpdatedData data)
        => data.Target.Id.ToString() == TargetId;

    private bool IsValidOverwrite(AuditOverwriteInfo overwriteInfo)
        => overwriteInfo.TargetId.ToString() == TargetId;

    public Dictionary<string, string> SerializeForLog()
    {
        return new Dictionary<string, string>
        {
            { nameof(TargetId), TargetId }
        };
    }
}
