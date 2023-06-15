using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using GrillBot.Core.Database;
using GrillBot.Core.Infrastructure;
using GrillBot.Database.Models;

namespace GrillBot.Data.Models.API.AuditLog.Filters;

public class AuditLogListParams : IQueryableModel<AuditLogItem>, IDictionaryObject
{
    public string? GuildId { get; set; }
    public List<string> ProcessedUserIds { get; set; } = new();
    public List<AuditLogItemType> Types { get; set; } = new();
    public List<AuditLogItemType> ExcludedTypes { get; set; } = new();
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public bool IgnoreBots { get; set; }
    public string? ChannelId { get; set; }

    /// <summary>
    /// Ids of records. Only number values, separated by ";".
    /// </summary>
    public string? Ids { get; set; }

    /// <summary>
    /// Show records that contains some file.
    /// </summary>
    public bool OnlyWithFiles { get; set; }

    /// <summary>
    /// Available: Guild, ProcessedUser, Type, Channel, CreatedAt.
    /// Default: CreatedAt.
    /// </summary>
    public SortParams? Sort { get; set; } = new() { OrderBy = "CreatedAt" };

    public IQueryable<AuditLogItem> SetIncludes(IQueryable<AuditLogItem> query)
    {
        return query
            .Include(o => o.Files)
            .Include(o => o.Guild)
            .Include(o => o.GuildChannel)
            .Include(o => o.ProcessedGuildUser!.User)
            .Include(o => o.ProcessedUser);
    }

    public IQueryable<AuditLogItem> SetQuery(IQueryable<AuditLogItem> query)
    {
        if (!string.IsNullOrEmpty(Ids))
        {
            var ids = Ids
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(long.Parse)
                .Where(o => o > 0)
                .ToList();

            return query.Where(o => ids.Contains(o.Id));
        }

        if (Types.Count > 0)
            query = query.Where(o => Types.Contains(o.Type));
        else if (ExcludedTypes.Count > 0)
            query = query.Where(o => !ExcludedTypes.Contains(o.Type));

        if (!string.IsNullOrEmpty(GuildId))
            query = query.Where(o => o.GuildId == GuildId);

        if (ProcessedUserIds.Count > 0)
            query = query.Where(o => o.ProcessedUserId != null && ProcessedUserIds.Contains(o.ProcessedUserId));

        if (CreatedFrom != null)
            query = query.Where(o => o.CreatedAt >= CreatedFrom);

        if (CreatedTo != null)
            query = query.Where(o => o.CreatedAt <= CreatedTo);

        if (IgnoreBots)
            query = query.Where(o => o.ProcessedUserId == null || (o.ProcessedUser!.Flags & (int)UserFlags.NotUser) == 0);

        if (!string.IsNullOrEmpty(ChannelId))
            query = query.Where(o => o.ChannelId == ChannelId);

        if (OnlyWithFiles)
            query = query.Where(o => o.Files.Any());

        return query;
    }

    public IQueryable<AuditLogItem> SetSort(IQueryable<AuditLogItem> query)
    {
        if (Sort == null)
            return query;

        var sortQuery = Sort.OrderBy switch
        {
            "Guild" => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.Guild!.Name),
                _ => query.OrderBy(o => o.Guild!.Name)
            },
            "ProcessedUser" => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.ProcessedGuildUser!.Nickname).ThenByDescending(o => o.ProcessedUser!.Username).ThenByDescending(o => o.ProcessedUser!.Discriminator),
                _ => query.OrderBy(o => o.ProcessedGuildUser!.Nickname).ThenBy(o => o.ProcessedUser!.Username)
            },
            "Type" => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.Type),
                _ => query.OrderBy(o => o.Type)
            },
            "Channel" => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.GuildChannel!.Name),
                _ => query.OrderBy(o => o.GuildChannel!.Name)
            },
            _ => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.CreatedAt),
                _ => query.OrderBy(o => o.CreatedAt)
            }
        };

        return Sort.Descending ? sortQuery.ThenByDescending(o => o.Id) : sortQuery.ThenBy(o => o.Id);
    }

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>();
    }
}
