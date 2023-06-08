using System;
using System.Collections.Generic;

namespace GrillBot.Data.Models.AuditLog;

public class MessageData
{
    public AuditUserInfo Author { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Content { get; set; }
    public List<EmbedInfo>? Embeds { get; set; } 
}
