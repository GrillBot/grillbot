using System.Collections.Generic;
using GrillBot.Core.Infrastructure;
using GrillBot.Database.Entity;
using Newtonsoft.Json;

namespace GrillBot.Data.Models.API.AuditLog.Filters;

public class TextFilter : IExtendedFilter, IDictionaryObject
{
    public string? Text { get; set; }

    public bool IsSet()
        => !string.IsNullOrEmpty(Text);

    public bool IsValid(AuditLogItem item, JsonSerializerSettings settings)
    {
        return item.Data.Contains(Text!);
    }

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>();
        if (IsSet())
            result.Add(nameof(Text), Text!);
        return result;
    }
}
