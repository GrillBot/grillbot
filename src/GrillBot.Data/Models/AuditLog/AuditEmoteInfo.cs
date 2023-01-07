using Discord.Rest;

namespace GrillBot.Data.Models.AuditLog;

public class AuditEmoteInfo
{
    public ulong Id { get; set; }
    public string EmoteId { get; set; }
    
    public string Name { get; set; }

    public AuditEmoteInfo() { }

    public AuditEmoteInfo(ulong id, string name)
    {
        EmoteId = id.ToString();
        Name = name;
    }

    public AuditEmoteInfo(EmoteDeleteAuditLogData data)
        : this(data.EmoteId, data.Name) { }
}
