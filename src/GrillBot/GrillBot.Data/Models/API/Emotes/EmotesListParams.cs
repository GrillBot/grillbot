using GrillBot.Database;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using GrillBot.Common.Extensions;
using GrillBot.Common.Infrastructure;
using GrillBot.Database.Models;

namespace GrillBot.Data.Models.API.Emotes;

public class EmotesListParams : IQueryableModel<EmoteStatisticItem>, IApiObject
{
    public string GuildId { get; set; }

    [OpenApiIgnore]
    [JsonIgnore]
    public string UserId { get; set; }

    public RangeParams<int?> UseCount { get; set; }
    public RangeParams<DateTime?> FirstOccurence { get; set; }
    public RangeParams<DateTime?> LastOccurence { get; set; }

    public bool FilterAnimated { get; set; }
    public string EmoteName { get; set; }

    /// <summary>
    /// Available: UseCount, FirstOccurence, LastOccurence, EmoteId.
    /// Default: UseCount
    /// </summary>
    public SortParams Sort { get; set; } = new() { OrderBy = "UseCount" };

    public PaginatedParams Pagination { get; set; } = new();

    public IQueryable<EmoteStatisticItem> SetIncludes(IQueryable<EmoteStatisticItem> query)
    {
        return query
            .Include(o => o.User.User)
            .Include(o => o.Guild);
    }

    public IQueryable<EmoteStatisticItem> SetQuery(IQueryable<EmoteStatisticItem> query)
    {
        if (!string.IsNullOrEmpty(GuildId))
            query = query.Where(o => o.GuildId == GuildId);

        if (!string.IsNullOrEmpty(UserId))
            query = query.Where(o => o.UserId == UserId);

        if (FilterAnimated)
            query = query.Where(o => !o.EmoteId.StartsWith("<a:"));

        if (!string.IsNullOrEmpty(EmoteName))
            query = query.Where(o => o.EmoteId.Contains($":{EmoteName}:"));

        return query;
    }

    public IQueryable<EmoteStatisticItem> SetSort(IQueryable<EmoteStatisticItem> query) => query;

    public Dictionary<string, string> SerializeForLog()
    {
        var result = new Dictionary<string, string>
        {
            { nameof(GuildId), GuildId },
            { nameof(FilterAnimated), FilterAnimated.ToString() },
            { nameof(EmoteName), EmoteName }
        };

        result.AddApiObject(UseCount, nameof(UseCount));
        result.AddApiObject(FirstOccurence, nameof(FirstOccurence));
        result.AddApiObject(LastOccurence, nameof(LastOccurence));
        return result;
    }
}
