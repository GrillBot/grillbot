using System;
using System.Collections.Generic;
using GrillBot.Core.Services.AuditLog.Models.Response.Search;
using GrillBot.Data.Models.API.Users;

namespace GrillBot.Data.Models.API.AuditLog.Preview;

public class MessageDeletedPreview
{
    public User User { get; set; } = null!;
    public DateTime MessageCreatedAt { get; set; }
    public int ContentLength { get; set; }
    public int EmbedCount { get; set; }
    public int EmbedFieldsCount { get; set; }
}
