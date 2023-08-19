using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using GrillBot.Core.Database;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Validation;
using GrillBot.Database.Models;
using GrillBot.Core.Extensions;

namespace GrillBot.Data.Models.API.Unverify;

/// <summary>
/// Paginated params of unverify logs
/// </summary>
public class UnverifyLogParams : IQueryableModel<UnverifyLog>, IDictionaryObject
{
    /// <summary>
    /// Selected operation.
    /// </summary>
    public UnverifyOperation? Operation { get; set; }

    /// <summary>
    /// Guild ID
    /// </summary>
    [DiscordId]
    public string? GuildId { get; set; }

    /// <summary>
    /// Who did operation. If user have lower permission, this property is ignored.
    /// </summary>
    [DiscordId]
    public string? FromUserId { get; set; }

    /// <summary>
    /// Who was target of operation. If user have lower permission, this property is ignored.
    /// </summary>
    [DiscordId]
    public string? ToUserId { get; set; }

    /// <summary>
    /// Range when operation did.
    /// </summary>
    public RangeParams<DateTime?>? Created { get; set; }

    /// <summary>
    /// Available: Operation, Guild, FromUser, ToUser, CreatedAt
    /// Default: CreatedAt.
    /// </summary>
    public SortParams Sort { get; set; } = new() { OrderBy = "CreatedAt" };

    public PaginatedParams Pagination { get; set; } = new();

    public IQueryable<UnverifyLog> SetIncludes(IQueryable<UnverifyLog> query)
    {
        return query
            .Include(o => o.FromUser!.User)
            .Include(o => o.ToUser!.User)
            .Include(o => o.Guild);
    }

    public IQueryable<UnverifyLog> SetQuery(IQueryable<UnverifyLog> query)
    {
        if (Operation != null)
            query = query.Where(o => o.Operation == Operation);

        if (!string.IsNullOrEmpty(GuildId))
            query = query.Where(o => o.GuildId == GuildId);

        if (!string.IsNullOrEmpty(FromUserId))
            query = query.Where(o => o.FromUserId == FromUserId);

        if (!string.IsNullOrEmpty(ToUserId))
            query = query.Where(o => o.ToUserId == ToUserId);

        if (Created?.From != null)
            query = query.Where(o => o.CreatedAt >= Created.From.Value);

        if (Created?.To != null)
            query = query.Where(o => o.CreatedAt <= Created.To.Value);

        return query;
    }

    public IQueryable<UnverifyLog> SetSort(IQueryable<UnverifyLog> query)
    {
        var sortQuery = Sort.OrderBy switch
        {
            "Operation" => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.Operation),
                _ => query.OrderBy(o => o.Operation)
            },
            "Guild" => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.Guild!.Name),
                _ => query.OrderBy(o => o.Guild!.Name)
            },
            "FromUser" => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.FromUser!.User!.Username),
                _ => query.OrderBy(o => o.FromUser!.User!.Username)
            },
            "ToUser" => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.ToUser!.User!.Username),
                _ => query.OrderBy(o => o.ToUser!.User!.Username)
            },
            _ => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.CreatedAt),
                _ => query.OrderBy(o => o.CreatedAt)
            },
        };

        return Sort.Descending ? sortQuery.ThenByDescending(o => o.Id) : sortQuery.ThenBy(o => o.Id);
    }

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(GuildId), GuildId },
            { nameof(FromUserId), FromUserId },
            { nameof(ToUserId), ToUserId },
        };

        if (Operation != null)
            result[nameof(Operation)] = $"{Operation} ({(int)Operation.Value})";
        result.MergeDictionaryObjects(Created, nameof(Created));
        result.MergeDictionaryObjects(Sort, nameof(Sort));
        result.MergeDictionaryObjects(Pagination, nameof(Pagination));
        return result;
    }
}
