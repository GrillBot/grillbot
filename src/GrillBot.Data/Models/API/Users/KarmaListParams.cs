using System.Collections.Generic;
using GrillBot.Common.Extensions;
using GrillBot.Common.Infrastructure;
using GrillBot.Common.Models.Pagination;

namespace GrillBot.Data.Models.API.Users;

public class KarmaListParams : IApiObject
{
    public PaginatedParams Pagination { get; set; } = new();

    public Dictionary<string, string> SerializeForLog()
    {
        var result = new Dictionary<string, string>();
        result.AddApiObject(Pagination, nameof(Pagination));

        return result;
    }
}
