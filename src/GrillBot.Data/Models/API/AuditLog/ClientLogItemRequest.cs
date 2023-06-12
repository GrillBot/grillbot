using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GrillBot.Core.Infrastructure;

namespace GrillBot.Data.Models.API.AuditLog;

public class ClientLogItemRequest : IDictionaryObject
{
    public bool IsInfo { get; set; }
    public bool IsError { get; set; }
    public bool IsWarning { get; set; }

    [Required]
    public string Content { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string AppName { get; set; } = null!;

    [Required]
    [StringLength(512)]
    public string Source { get; set; } = null!;

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(IsInfo), IsInfo.ToString() },
            { nameof(IsError), IsError.ToString() },
            { nameof(IsWarning), IsWarning.ToString() },
            { nameof(Content), Content },
            { nameof(AppName), AppName },
            { nameof(Source), Source }
        };
    }
}
