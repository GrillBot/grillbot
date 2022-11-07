using GrillBot.Data.Infrastructure.Validation;
using GrillBot.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using GrillBot.Common.Extensions;
using GrillBot.Common.Infrastructure;
using GrillBot.Database.Models;

namespace GrillBot.Data.Models.API.Invites;

public class GetInviteListParams : IQueryableModel<Database.Entity.Invite>, IApiObject
{
    [DiscordId]
    public string GuildId { get; set; }

    [DiscordId]
    public string CreatorId { get; set; }

    public string Code { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }

    /// <summary>
    /// Available: Code, CreatedAt, Creator, UseCount.
    /// Default: Code
    /// </summary>
    public SortParams Sort { get; set; } = new() { OrderBy = "Code" };

    public PaginatedParams Pagination { get; set; } = new();

    public IQueryable<Database.Entity.Invite> SetIncludes(IQueryable<Database.Entity.Invite> query)
    {
        return query
            .Include(o => o.Creator.User)
            .Include(o => o.UsedUsers)
            .Include(o => o.Guild);
    }

    public IQueryable<Database.Entity.Invite> SetQuery(IQueryable<Database.Entity.Invite> query)
    {
        query = query.Where(o => o.UsedUsers.Count > 0);

        if (!string.IsNullOrEmpty(GuildId))
            query = query.Where(o => o.GuildId == GuildId);

        if (!string.IsNullOrEmpty(CreatorId))
            query = query.Where(o => o.CreatorId == CreatorId);

        if (!string.IsNullOrEmpty(Code))
            query = query.Where(o => o.Code.Contains(Code));

        if (CreatedFrom != null)
            query = query.Where(o => o.CreatedAt >= CreatedFrom.Value);

        if (CreatedTo != null)
            query = query.Where(o => o.CreatedAt <= CreatedTo.Value);

        return query;
    }

    public IQueryable<Database.Entity.Invite> SetSort(IQueryable<Database.Entity.Invite> query)
    {
        return Sort.OrderBy switch
        {
            "CreatedAt" => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.CreatedAt),
                _ => query.OrderBy(o => o.CreatedAt)
            },
            "Creator" => Sort.Descending switch
            {
                true => query.OrderByDescending(o => !string.IsNullOrEmpty(o.Creator.Nickname) ? o.Creator.Nickname : o.Creator.User.Username),
                _ => query.OrderBy(o => !string.IsNullOrEmpty(o.Creator.Nickname) ? o.Creator.Nickname : o.Creator.User.Username),
            },
            "UseCount" => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.UsedUsers.Count).ThenByDescending(o => o.Code),
                _ => query.OrderBy(o => o.UsedUsers.Count).ThenBy(o => o.Code)
            },
            _ => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.Code),
                _ => query.OrderBy(o => o.Code)
            }
        };
    }

    public Dictionary<string, string> SerializeForLog()
    {
        var result = new Dictionary<string, string>
        {
            { nameof(GuildId), GuildId },
            { nameof(CreatorId), CreatorId },
            { nameof(Code), Code },
            { nameof(CreatedFrom), CreatedFrom?.ToString("o") },
            { nameof(CreatedTo), CreatedTo?.ToString("o") }
        };

        result.AddApiObject(Sort, nameof(Sort));
        result.AddApiObject(Pagination, nameof(Pagination));
        return result;
    }
}
