using System.Collections.Generic;
using System.Linq;
using GrillBot.Core.Database;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models.Pagination;

namespace GrillBot.Data.Models.API.Guilds;

public class GetGuildListParams : IQueryableModel<Database.Entity.Guild>, IDictionaryObject
{
    public string? NameQuery { get; set; }
    public PaginatedParams Pagination { get; set; } = new();

    public IQueryable<Database.Entity.Guild> SetIncludes(IQueryable<Database.Entity.Guild> query) => query;

    public IQueryable<Database.Entity.Guild> SetQuery(IQueryable<Database.Entity.Guild> query)
    {
        if (!string.IsNullOrEmpty(NameQuery))
            query = query.Where(o => o.Name.Contains(NameQuery));

        return query;
    }

    public IQueryable<Database.Entity.Guild> SetSort(IQueryable<Database.Entity.Guild> query)
    {
        return query.OrderBy(o => o.Name);
    }

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(NameQuery), NameQuery }
        };

        result.MergeDictionaryObjects(Pagination, nameof(Pagination));
        return result;
    }
}
