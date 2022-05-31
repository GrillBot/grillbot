using GrillBot.Database.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace GrillBot.Data.Models.API.AuditLog;

public class ClientLogItemRequest : IValidatableObject
{
    public bool IsInfo { get; set; }
    public bool IsError { get; set; }
    public bool IsWarning { get; set; }

    public string Content { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var flags = new[] { IsInfo, IsWarning, IsError };

        if (!flags.Any(o => o))
            yield return new ValidationResult("One of log type method is required.");
        else if (flags.Count(o => o) > 1)
            yield return new ValidationResult("Multiple selected log types. Select only one please.");
    }

    public AuditLogItemType GetAuditLogType()
    {
        if (IsError) return AuditLogItemType.Error;
        if (IsWarning) return AuditLogItemType.Warning;

        return AuditLogItemType.Info;
    }
}
