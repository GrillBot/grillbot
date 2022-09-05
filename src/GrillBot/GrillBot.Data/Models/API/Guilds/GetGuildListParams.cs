using System.Collections.Generic;
using GrillBot.Database;
using System.Linq;
using GrillBot.Common.Extensions;
using GrillBot.Common.Infrastructure;
using GrillBot.Database.Models;
using Newtonsoft.Json;
using NSwag.Annotations;

namespace GrillBot.Data.Models.API.Guilds;

public class GetGuildListParams : IQueryableModel<Database.Entity.Guild>, IApiObject
{
    public string NameQuery { get; set; }
    public PaginatedParams Pagination { get; set; } = new();

    [JsonIgnore]
    [OpenApiIgnore]
    public List<string> MutualGuildIds { get; set; } = new();

    public IQueryable<Database.Entity.Guild> SetIncludes(IQueryable<Database.Entity.Guild> query) => query;

    public IQueryable<Database.Entity.Guild> SetQuery(IQueryable<Database.Entity.Guild> query)
    {
        if (!string.IsNullOrEmpty(NameQuery))
            query = query.Where(o => o.Name.Contains(NameQuery));

        if (MutualGuildIds.Count > 0)
            query = query.Where(o => MutualGuildIds.Contains(o.Id));

        return query;
    }

    public IQueryable<Database.Entity.Guild> SetSort(IQueryable<Database.Entity.Guild> query)
    {
        return query.OrderBy(o => o.Name);
    }

    public Dictionary<string, string> SerializeForLog()
    {
        var result = new Dictionary<string, string>
        {
            { nameof(NameQuery), NameQuery }
        };

        result.AddApiObject(Pagination, nameof(Pagination));
        return result;
    }
}
