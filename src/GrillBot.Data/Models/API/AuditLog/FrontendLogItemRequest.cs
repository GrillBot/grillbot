using Discord;
using GrillBot.Core.Infrastructure;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Data.Models.API.AuditLog;

public class FrontendLogItemRequest : IDictionaryObject
{
    public LogSeverity Severity { get; set; } = LogSeverity.Info;
    public string Message { get; set; } = null!;

    [StringLength(512)]
    public string Source { get; set; } = "GlobalHandler";

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(Severity), Severity.ToString() },
            { "Message.Length", Message.Length.ToString() },
            { nameof(Source), Source }
        };
    }
}
