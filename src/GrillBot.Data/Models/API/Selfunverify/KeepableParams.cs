using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GrillBot.Common.Infrastructure;

namespace GrillBot.Data.Models.API.Selfunverify;

public class KeepableParams : IApiObject
{
    public string Group { get; set; } = "_";

    [Required]
    public string Name { get; set; }

    public Dictionary<string, string> SerializeForLog()
    {
        return new Dictionary<string, string>
        {
            { nameof(Group), Group },
            { nameof(Name), Name }
        };
    }
}
