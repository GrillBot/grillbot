using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GrillBot.Core.Infrastructure;

namespace GrillBot.Data.Models.API.AutoReply;

public class AutoReplyItemParams : IDictionaryObject
{
    [Required]
    public string Template { get; set; } = null!;

    [Required]
    public string Reply { get; set; } = null!;

    public long Flags { get; set; }

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(Template), Template },
            { nameof(Reply), Reply },
            { nameof(Flags), Flags.ToString() }
        };
    }
}
