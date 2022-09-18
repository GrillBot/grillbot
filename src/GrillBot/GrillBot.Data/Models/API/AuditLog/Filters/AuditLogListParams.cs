using GrillBot.Data.Infrastructure.Validation;
using GrillBot.Database;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using GrillBot.Common.Extensions;
using GrillBot.Common.Infrastructure;
using GrillBot.Database.Models;

namespace GrillBot.Data.Models.API.AuditLog.Filters;

public class AuditLogListParams : IQueryableModel<AuditLogItem>, IApiObject
{
    [DiscordId]
    public string GuildId { get; set; }

    [DiscordId]
    public List<string> ProcessedUserIds { get; set; } = new();

    public List<AuditLogItemType> Types { get; set; } = new();
    public List<AuditLogItemType> ExcludedTypes { get; set; } = new();
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
    public TargetIdFilter OverwriteCreatedFilter { get; set; }
    public TargetIdFilter OverwriteDeletedFilter { get; set; }
    public TargetIdFilter OverwriteUpdatedFilter { get; set; }
    public TargetIdFilter MemberRolesUpdatedFilter { get; set; }
    public TargetIdFilter MemberUpdatedFilter { get; set; }
    public bool OnlyFromStart { get; set; }

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
        var conditions = new Func<bool>[]
        {
            () => Types.Contains(AuditLogItemType.Info) && InfoFilter?.IsSet() == true,
            () => Types.Contains(AuditLogItemType.Warning) && WarningFilter?.IsSet() == true,
            () => Types.Contains(AuditLogItemType.Error) && ErrorFilter?.IsSet() == true,
            () => Types.Contains(AuditLogItemType.Command) && CommandFilter?.IsSet() == true,
            () => Types.Contains(AuditLogItemType.InteractionCommand) && InteractionFilter?.IsSet() == true,
            () => Types.Contains(AuditLogItemType.JobCompleted) && JobFilter?.IsSet() == true,
            () => Types.Contains(AuditLogItemType.Api) && ApiRequestFilter?.IsSet() == true,
            () => Types.Contains(AuditLogItemType.OverwriteCreated) && OverwriteCreatedFilter?.IsSet() == true,
            () => Types.Contains(AuditLogItemType.OverwriteDeleted) && OverwriteDeletedFilter?.IsSet() == true,
            () => Types.Contains(AuditLogItemType.OverwriteUpdated) && OverwriteUpdatedFilter?.IsSet() == true,
            () => Types.Contains(AuditLogItemType.MemberUpdated) && MemberUpdatedFilter?.IsSet() == true,
            () => Types.Contains(AuditLogItemType.MemberRoleUpdated) && MemberRolesUpdatedFilter?.IsSet() == true
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

        if (ExcludedTypes.Count > 0)
            query = query.Where(o => !ExcludedTypes.Contains(o.Type));

        if (!string.IsNullOrEmpty(GuildId))
            query = query.Where(o => o.GuildId == GuildId);

        if (ProcessedUserIds.Count > 0)
            query = query.Where(o => ProcessedUserIds.Contains(o.ProcessedUserId));

        if (CreatedFrom != null)
            query = query.Where(o => o.CreatedAt >= CreatedFrom);

        if (CreatedTo != null)
            query = query.Where(o => o.CreatedAt <= CreatedTo);

        if (IgnoreBots)
            query = query.Where(o => o.ProcessedUserId == null || (o.ProcessedUser.Flags & (int)UserFlags.NotUser) == 0);

        if (!string.IsNullOrEmpty(ChannelId))
            query = query.Where(o => o.ChannelId == ChannelId);

        if (string.IsNullOrEmpty(Ids))
            return query;

        var ids = Ids
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(long.Parse)
            .Where(o => o > 0)
            .ToList();

        return query.Where(o => ids.Contains(o.Id));
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

        return Sort.Descending ? sortQuery.ThenByDescending(o => o.Id) : sortQuery.ThenBy(o => o.Id);
    }

    public void UpdateStartDate(DateTime startAt)
    {
        if (!OnlyFromStart) return;

        CreatedFrom = startAt;
        if (CreatedTo <= startAt)
            CreatedTo = null;
    }

    public Dictionary<string, string> SerializeForLog()
    {
        var result = new Dictionary<string, string>
        {
            { nameof(GuildId), GuildId },
            { nameof(CreatedFrom), CreatedFrom?.ToString("o") },
            { nameof(CreatedTo), CreatedTo?.ToString("o") },
            { nameof(IgnoreBots), IgnoreBots.ToString() },
            { nameof(ChannelId), ChannelId },
            { nameof(OnlyFromStart), OnlyFromStart.ToString() },
            { nameof(Ids), Ids }
        };

        for (var i = 0; i < ProcessedUserIds.Count; i++)
            result.Add($"{nameof(ProcessedUserIds)}[{i}]", ProcessedUserIds[i]);
        for (var i = 0; i < Types.Count; i++)
            result.Add($"{nameof(Types)}[{i}]", $"{Types[i]} ({(int)Types[i]})");
        for (var i = 0; i < ExcludedTypes.Count; i++)
            result.Add($"{nameof(ExcludedTypes)}[{i}]", $"{ExcludedTypes[i]} ({(int)ExcludedTypes[i]})");

        result.AddApiObject(InfoFilter, nameof(InfoFilter));
        result.AddApiObject(WarningFilter, nameof(WarningFilter));
        result.AddApiObject(ErrorFilter, nameof(ErrorFilter));
        result.AddApiObject(CommandFilter, nameof(CommandFilter));
        result.AddApiObject(InteractionFilter, nameof(InteractionFilter));
        result.AddApiObject(JobFilter, nameof(JobFilter));
        result.AddApiObject(ApiRequestFilter, nameof(ApiRequestFilter));
        result.AddApiObject(OverwriteCreatedFilter, nameof(OverwriteCreatedFilter));
        result.AddApiObject(OverwriteDeletedFilter, nameof(OverwriteDeletedFilter));
        result.AddApiObject(OverwriteUpdatedFilter, nameof(OverwriteUpdatedFilter));
        result.AddApiObject(MemberUpdatedFilter, nameof(MemberUpdatedFilter));
        result.AddApiObject(Sort, nameof(Sort));
        result.AddApiObject(Pagination, nameof(Pagination));

        return result;
    }
}
