﻿using System;
using System.Linq;
using GrillBot.Data.Infrastructure.Validation;
using GrillBot.Database;
using GrillBot.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Data.Models.API.Suggestions;

public class GetSuggestionsListParams : IQueryableModel<Database.Entity.EmoteSuggestion>
{
    public RangeParams<DateTime?> CreatedAt { get; set; }

    [DiscordId]
    public string GuildId { get; set; }

    [DiscordId]
    public string FromUserId { get; set; }

    public string EmoteName { get; set; }
    public bool OnlyApprovedToVote { get; set; }
    public bool OnlyUnfinishedVotes { get; set; }
    public bool OnlyCommunityApproved { get; set; }
    public SortParams Sort { get; set; } = new();
    public PaginatedParams Pagination { get; set; } = new();

    public IQueryable<Database.Entity.EmoteSuggestion> SetQuery(IQueryable<Database.Entity.EmoteSuggestion> query)
    {
        if (CreatedAt != null)
        {
            if (CreatedAt.From != null)
                query = query.Where(o => o.CreatedAt >= CreatedAt.From.Value);

            if (CreatedAt.To != null)
                query = query.Where(o => o.CreatedAt < CreatedAt.To.Value);
        }

        if (!string.IsNullOrEmpty(GuildId))
            query = query.Where(o => o.GuildId == GuildId);

        if (!string.IsNullOrEmpty(FromUserId))
            query = query.Where(o => o.FromUserId == FromUserId);

        if (!string.IsNullOrEmpty(EmoteName))
            query = query.Where(o => o.EmoteName.Contains(EmoteName, StringComparison.OrdinalIgnoreCase));

        if (OnlyApprovedToVote)
            query = query.Where(o => o.ApprovedForVote == true);

        if (OnlyUnfinishedVotes)
            query = query.Where(o => !o.VoteFinished);

        if (OnlyCommunityApproved)
            query = query.Where(o => o.CommunityApproved);

        return query;
    }

    public IQueryable<Database.Entity.EmoteSuggestion> SetIncludes(IQueryable<Database.Entity.EmoteSuggestion> query)
    {
        return query
            .Include(o => o.Guild)
            .Include(o => o.FromUser!.User);
    }

    public IQueryable<Database.Entity.EmoteSuggestion> SetSort(IQueryable<Database.Entity.EmoteSuggestion> query)
    {
        return Sort.Descending ? query.OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.Id) : query.OrderBy(o => o.CreatedAt).ThenBy(o => o.Id);
    }
}
