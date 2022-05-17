using GrillBot.Database.Entity;
using Newtonsoft.Json;

namespace GrillBot.Data.Models.API.AuditLog.Filters;

public class TextFilter : IExtendedFilter
{
    public string Text { get; set; }

    public bool IsSet()
        => !string.IsNullOrEmpty(Text);

    public bool IsValid(AuditLogItem item, JsonSerializerSettings settings)
    {
        return item.Data.Contains(Text);
    }
}
