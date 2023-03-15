using System.Collections.Generic;
using GrillBot.Core.Infrastructure;

namespace GrillBot.Database.Models;

public class SortParams : IDictionaryObject
{
    public string? OrderBy { get; set; }
    public bool Descending { get; set; }

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(OrderBy), OrderBy ?? "" },
            { nameof(Descending), Descending.ToString() }
        };
    }
}
