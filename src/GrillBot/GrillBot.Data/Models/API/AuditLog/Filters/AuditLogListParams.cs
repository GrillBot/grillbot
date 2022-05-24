using GrillBot.Data.Infrastructure.Validation;
using GrillBot.Data.Models.API.Common;
using GrillBot.Database;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace GrillBot.Data.Models.API.AuditLog.Filters;

public class AuditLogListParams : IQueryableModel<AuditLogItem>, IValidatableObject
{
    [DiscordId]
    public string GuildId { get; set; }

    [DiscordId]
    public List<string> ProcessedUserIds { get; set; }
    public List<AuditLogItemType> Types { get; set; } = new();
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public bool IgnoreBots { get; set; }

    [DiscordId]
    public string ChannelId { get; set; }

    public TextFilter InfoFilter { get; set; }
    public TextFilter WarningFilter { get; set; }
    public TextFilter ErrorFilter { get; set; }
    public ExecutionFilter CommandFilter { get; set; }
    public ExecutionFilter InteractionFilter { get; set; }
    public ExecutionFilter JobFilter { get; set; }
    public ApiRequestFilter ApiRequestFilter { get; set; }

    /// <summary>
    /// Ids of records. Only number values, separated by ";".
    /// </summary>
    public string Ids { get; set; }

    /// <summary>
    /// Available: Guild, ProcessedUser, Type, Channel, CreatedAt.
    /// Default: CreatedAt.
    /// </summary>
    public SortParams Sort { get; set; } = new() { OrderBy = "CreatedAt" };
    public PaginatedParams Pagination { get; set; } = new();

    public bool AnyExtendedFilter()
    {
        var conditions = new[]
        {
            () => Types.Contains(AuditLogItemType.Info) && InfoFilter?.IsSet() == true,
            () => Types.Contains(AuditLogItemType.Warning) && WarningFilter?.IsSet() == true,
            () => Types.Contains(AuditLogItemType.Error) && ErrorFilter?.IsSet() == true,
            () => Types.Contains(AuditLogItemType.Command) && CommandFilter?.IsSet() == true,
            () => Types.Contains(AuditLogItemType.InteractionCommand) && InteractionFilter?.IsSet() == true,
            () => Types.Contains(AuditLogItemType.JobCompleted) && JobFilter?.IsSet() == true,
            () => Types.Contains(AuditLogItemType.API) && ApiRequestFilter?.IsSet() == true
        };

        return conditions.Any(o => o());
    }

    public IQueryable<AuditLogItem> SetIncludes(IQueryable<AuditLogItem> query)
    {
        return query
            .Include(o => o.Files)
            .Include(o => o.Guild)
            .Include(o => o.GuildChannel)
            .Include(o => o.ProcessedGuildUser.User)
            .Include(o => o.ProcessedUser);
    }

    public IQueryable<AuditLogItem> SetQuery(IQueryable<AuditLogItem> query)
    {
        if (Types.Count > 0)
            query = query.Where(o => Types.Contains(o.Type));

        if (!string.IsNullOrEmpty(GuildId))
            query = query.Where(o => o.GuildId == GuildId);

        if (ProcessedUserIds?.Count > 0)
            query = query.Where(o => ProcessedUserIds.Contains(o.ProcessedUserId));

        if (CreatedFrom != null)
            query = query.Where(o => o.CreatedAt >= CreatedFrom);

        if (CreatedTo != null)
            query = query.Where(o => o.CreatedAt <= CreatedTo);

        if (IgnoreBots)
            query = query.Where(o => o.ProcessedUserId == null || (o.ProcessedGuildUser.User.Flags & (int)UserFlags.NotUser) == 0);

        if (!string.IsNullOrEmpty(ChannelId))
            query = query.Where(o => o.ChannelId == ChannelId);

        if (!string.IsNullOrEmpty(Ids))
        {
            var ids = Ids
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(o => long.Parse(o))
                .Where(o => o > 0)
                .ToList();

            query = query.Where(o => ids.Contains(o.Id));
        }

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
                true => query.OrderByDescending(o => o.Guild.Name),
                _ => query.OrderBy(o => o.Guild.Name)
            },
            "ProcessedUser" => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.ProcessedGuildUser.Nickname).ThenByDescending(o => o.ProcessedUser.Username).ThenByDescending(o => o.ProcessedUser.Discriminator),
                _ => query.OrderBy(o => o.ProcessedGuildUser.Nickname).ThenBy(o => o.ProcessedUser.Username)
            },
            "Type" => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.Type),
                _ => query.OrderBy(o => o.Type)
            },
            "Channel" => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.GuildChannel.Name),
                _ => query.OrderBy(o => o.GuildChannel.Name)
            },
            _ => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.CreatedAt),
                _ => query.OrderBy(o => o.CreatedAt)
            }
        };

        if (Sort.Descending)
            return sortQuery.ThenByDescending(o => o.Id);
        else
            return sortQuery.ThenBy(o => o.Id);
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrEmpty(Ids))
        {
            var items = Ids.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            for (int i = 0; i < items.Length; i++)
            {
                if (!long.TryParse(items[i], out var _))
                    yield return new ValidationResult($"ID[{i}] is not number.", new[] { $"{nameof(Ids)}[{i}]" });
            }
        }
    }
}
