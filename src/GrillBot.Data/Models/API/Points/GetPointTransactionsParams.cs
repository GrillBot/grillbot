using System;
using System.Collections.Generic;
using System.Linq;
using GrillBot.Common.Extensions;
using GrillBot.Core.Database;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Validation;
using GrillBot.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Data.Models.API.Points;

public class GetPointTransactionsParams : IQueryableModel<Database.Entity.PointsTransaction>, IDictionaryObject
{
    /// <summary>
    /// Show or ignore merged items.
    /// Default: false
    /// </summary>
    public bool Merged { get; set; }

    [DiscordId]
    public string? GuildId { get; set; }

    [DiscordId]
    public string? UserId { get; set; }

    public RangeParams<DateTime?>? AssignedAt { get; set; }
    public bool OnlyReactions { get; set; }
    public bool OnlyMessages { get; set; }
    public string? MessageId { get; set; }

    /// <summary>
    /// Available: AssignedAt, User, Points
    /// Default: AssignedAt
    /// </summary>
    public SortParameters Sort { get; set; } = new() { OrderBy = "AssignedAt", Descending = true };

    public PaginatedParams Pagination { get; set; } = new();

    public IQueryable<Database.Entity.PointsTransaction> SetIncludes(IQueryable<Database.Entity.PointsTransaction> query)
    {
        return query
            .Include(o => o.Guild)
            .Include(o => o.GuildUser.User);
    }

    public IQueryable<Database.Entity.PointsTransaction> SetQuery(IQueryable<Database.Entity.PointsTransaction> query)
    {
        // Show only merged or non-merged transactions.
        query = Merged ? query.Where(o => o.MergedItemsCount > 0) : query.Where(o => o.MergedItemsCount == 0);

        if (!string.IsNullOrEmpty(GuildId))
            query = query.Where(o => o.GuildId == GuildId);

        if (!string.IsNullOrEmpty(UserId))
            query = query.Where(o => o.UserId == UserId);

        if (AssignedAt != null)
        {
            if (AssignedAt.From != null)
                query = query.Where(o => o.AssingnedAt >= AssignedAt.From.Value);

            if (AssignedAt.To != null)
                query = query.Where(o => o.AssingnedAt < AssignedAt.To.Value);
        }

        if (OnlyReactions)
            query = query.Where(o => o.ReactionId != "");
        if (OnlyMessages)
            query = query.Where(o => o.ReactionId == "");
        if (!Merged && !string.IsNullOrEmpty(MessageId))
            query = query.Where(o => o.MessageId == MessageId);

        return query;
    }

    public IQueryable<Database.Entity.PointsTransaction> SetSort(IQueryable<Database.Entity.PointsTransaction> query)
    {
        return Sort.OrderBy switch
        {
            "User" => Sort.Descending
                ? query.OrderByDescending(o => o.GuildUser.Nickname).ThenByDescending(o => o.GuildUser.User!.Username).ThenByDescending(o => o.GuildUser.User!.Discriminator)
                    .ThenByDescending(o => o.AssingnedAt)
                : query.OrderBy(o => o.GuildUser.Nickname).ThenBy(o => o.GuildUser.User!.Username).ThenBy(o => o.GuildUser.User!.Discriminator).ThenBy(o => o.AssingnedAt),
            "Points" => Sort.Descending ? query.OrderByDescending(o => o.Points).ThenByDescending(o => o.AssingnedAt) : query.OrderBy(o => o.Points).ThenBy(o => o.AssingnedAt),
            _ => Sort.Descending ? query.OrderByDescending(o => o.AssingnedAt) : query.OrderBy(o => o.AssingnedAt)
        };
    }

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(Merged), Merged.ToString() },
            { nameof(GuildId), GuildId },
            { nameof(UserId), UserId },
            { nameof(OnlyReactions), OnlyReactions.ToString() },
            { nameof(OnlyMessages), OnlyMessages.ToString() },
            { nameof(MessageId), MessageId }
        };

        result.MergeDictionaryObjects(AssignedAt, nameof(AssignedAt));
        result.MergeDictionaryObjects(Sort, nameof(Sort));
        result.MergeDictionaryObjects(Pagination, nameof(Pagination));
        return result;
    }
}
