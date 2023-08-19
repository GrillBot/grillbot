using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GrillBot.Core.Infrastructure;

namespace GrillBot.Data.Models.API.ApiClients;

public class ApiClientParams : IDictionaryObject
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    public List<string> AllowedMethods { get; set; } = new();

    [Required]
    public bool Disabled { get; set; }

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(Name), Name },
            { nameof(Disabled), Disabled.ToString() }
        };

        for (var i = 0; i < AllowedMethods.Count; i++)
            result.Add($"AllowedMethods[{i}]", AllowedMethods[i]);
        return result;
    }
}
