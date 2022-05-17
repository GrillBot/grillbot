using GrillBot.Database.Entity;
using Newtonsoft.Json;

namespace GrillBot.Data.Models.API.AuditLog.Filters;

public interface IExtendedFilter
{
    bool IsSet();
    bool IsValid(AuditLogItem item, JsonSerializerSettings settings);
}
