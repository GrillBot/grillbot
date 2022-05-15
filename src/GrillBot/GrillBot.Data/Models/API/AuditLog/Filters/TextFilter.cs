using GrillBot.Database.Entity;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Data.Models.API.AuditLog.Filters;

public class TextFilter
{
    public string Text { get; set; }

    public bool IsValid(AuditLogItem item)
    {
        return item.Data.Contains(Text);
    }
}
