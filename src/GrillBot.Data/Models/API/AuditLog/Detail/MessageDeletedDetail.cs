using System;
using System.Collections.Generic;
using GrillBot.Common.Services.AuditLog.Models.Response.Detail;
using GrillBot.Data.Models.API.Users;

namespace GrillBot.Data.Models.API.AuditLog.Detail;

public class MessageDeletedDetail
{
    public User Author { get; set; } = null!;
    public DateTime MessageCreatedAt { get; set; }
    public string? Content { get; set; }
    public List<EmbedDetail> Embeds { get; set; } = new();
}
