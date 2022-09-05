using System;
using System.Collections.Generic;
using System.Linq;
using GrillBot.Common.Extensions;
using GrillBot.Common.Infrastructure;
using GrillBot.Database;
using GrillBot.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Data.Models.API.Points;

public class GetPointsSummaryParams : IQueryableModel<Database.Entity.PointsTransactionSummary>, IApiObject
{
    /// <summary>
    /// Show merged or non-merged items.
    /// </summary>
    public bool Merged { get; set; }

    public string GuildId { get; set; }
    public string UserId { get; set; }
    public RangeParams<DateTime?> Days { get; set; }

    /// <summary>
    /// Available: Day, MessagePoints, ReactionPoints
    /// Default: Day
    /// </summary>
    public SortParams Sort { get; set; } = new() { OrderBy = "Day", Descending = true };

    public PaginatedParams Pagination { get; set; } = new();

    public IQueryable<Database.Entity.PointsTransactionSummary> SetIncludes(IQueryable<Database.Entity.PointsTransactionSummary> query)
    {
        return query
            .Include(o => o.Guild)
            .Include(o => o.GuildUser.User);
    }

    public IQueryable<Database.Entity.PointsTransactionSummary> SetQuery(IQueryable<Database.Entity.PointsTransactionSummary> query)
    {
        query = query.Where(o => o.IsMerged == Merged);

        if (!string.IsNullOrEmpty(GuildId))
            query = query.Where(o => o.GuildId == GuildId);

        if (!string.IsNullOrEmpty(UserId))
            query = query.Where(o => o.UserId == UserId);

        if (Days == null)
            return query;

        if (Days.From != null)
            query = query.Where(o => o.Day >= Days.From.Value);

        if (Days.To != null)
            query = query.Where(o => o.Day < Days.To.Value);

        return query;
    }

    public IQueryable<Database.Entity.PointsTransactionSummary> SetSort(IQueryable<Database.Entity.PointsTransactionSummary> query)
    {
        return Sort.OrderBy switch
        {
            "MessagePoints" => Sort.Descending ? query.OrderByDescending(o => o.MessagePoints).ThenByDescending(o => o.Day) : query.OrderBy(o => o.MessagePoints).ThenBy(o => o.Day),
            "ReactionPoints" => Sort.Descending ? query.OrderByDescending(o => o.ReactionPoints).ThenByDescending(o => o.Day) : query.OrderBy(o => o.ReactionPoints).ThenBy(o => o.Day),
            _ => Sort.Descending ? query.OrderByDescending(o => o.Day) : query.OrderBy(o => o.Day)
        };
    }

    public Dictionary<string, string> SerializeForLog()
    {
        var result = new Dictionary<string, string>
        {
            { nameof(Merged), Merged.ToString() },
            { nameof(GuildId), GuildId },
            { nameof(UserId), UserId },
        };

        result.AddApiObject(Days, nameof(Days));
        result.AddApiObject(Sort, nameof(Sort));
        result.AddApiObject(Pagination, nameof(Pagination));
        return result;
    }
}
