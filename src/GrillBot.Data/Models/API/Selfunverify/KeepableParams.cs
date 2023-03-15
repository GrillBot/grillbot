using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GrillBot.Core.Infrastructure;

namespace GrillBot.Data.Models.API.Selfunverify;

public class KeepableParams : IDictionaryObject
{
    public string Group { get; set; } = "_";

    [Required]
    public string Name { get; set; } = null!;

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(Group), Group },
            { nameof(Name), Name }
        };
    }
}
