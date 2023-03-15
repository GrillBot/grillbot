using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GrillBot.Core.Infrastructure;

namespace GrillBot.Data.Models.API.Unverify;

public class UpdateUnverifyParams : IDictionaryObject
{
    [Required]
    public DateTime EndAt { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(EndAt), EndAt.ToString("o") },
            { nameof(Reason), Reason }
        };
    }
}
