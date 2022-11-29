using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GrillBot.Common.Infrastructure;

namespace GrillBot.Data.Models.API.ApiClients;

public class ApiClientParams : IApiObject
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    public List<string> AllowedMethods { get; set; } = new();

    public Dictionary<string, string> SerializeForLog()
    {
        var result = new Dictionary<string, string>
        {
            { nameof(Name), Name }
        };

        for (var i = 0; i < AllowedMethods.Count; i++)
            result.Add($"AllowedMethods[{i}]", AllowedMethods[i]);
        return result;
    }
}
