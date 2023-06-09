using System;
using System.Collections.Generic;
using GrillBot.Common.Services.AuditLog.Models.Response.Search;
using GrillBot.Data.Models.API.Users;

namespace GrillBot.Data.Models.API.AuditLog.Preview;

public class MessageDeletedPreview
{
    public User User { get; set; } = null!;
    public DateTime MessageCreatedAt { get; set; }
    public string? Content { get; set; }
    public List<EmbedPreview> Embeds { get; set; } = new();
}
