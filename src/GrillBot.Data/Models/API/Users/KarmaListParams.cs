using System.Collections.Generic;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models.Pagination;

namespace GrillBot.Data.Models.API.Users;

public class KarmaListParams : IDictionaryObject
{
    public PaginatedParams Pagination { get; set; } = new();

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>();
        result.MergeDictionaryObjects(Pagination, nameof(Pagination));

        return result;
    }
}
