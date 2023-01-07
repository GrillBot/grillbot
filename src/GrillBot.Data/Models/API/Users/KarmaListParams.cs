using System.Collections.Generic;
using GrillBot.Common.Extensions;
using GrillBot.Common.Infrastructure;
using GrillBot.Database.Models;

namespace GrillBot.Data.Models.API.Users;

public class KarmaListParams : IApiObject
{
    public SortParams Sort { get; set; } = new() { OrderBy = "Karma" };
    public PaginatedParams Pagination { get; set; } = new();

    public Dictionary<string, string> SerializeForLog()
    {
        var result = new Dictionary<string, string>();
        result.AddApiObject(Sort, nameof(Sort));
        result.AddApiObject(Pagination, nameof(Pagination));

        return result;
    }
}
