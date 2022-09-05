using System.Collections.Generic;
using GrillBot.Common.Infrastructure;

namespace GrillBot.Database.Models;

public class SortParams : IApiObject
{
    public string? OrderBy { get; set; }
    public bool Descending { get; set; }

    public Dictionary<string, string> SerializeForLog()
    {
        return new Dictionary<string, string>
        {
            { nameof(OrderBy), OrderBy ?? "" },
            { nameof(Descending), Descending.ToString() }
        };
    }
}
