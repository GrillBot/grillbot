using System.Collections.Generic;
using Discord;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using GrillBot.Common.Extensions;
using GrillBot.Core.Database;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Validation;
using GrillBot.Database.Models;

namespace GrillBot.Data.Models.API.Channels;

public class GetChannelListParams : IQueryableModel<GuildChannel>, IDictionaryObject
{
    [DiscordId]
    public string? GuildId { get; set; }

    public string? NameContains { get; set; }
    public ChannelType? ChannelType { get; set; }

    public bool HideDeleted { get; set; }

    /// <summary>
    /// Available: Name, Type, MessageCount,RolePermissions,UserPermissions
    /// </summary>
    public SortParams Sort { get; set; } = new() { OrderBy = "Name" };

    public PaginatedParams Pagination { get; } = new();

    public IQueryable<GuildChannel> SetIncludes(IQueryable<GuildChannel> query)
    {
        return query
            .Include(o => o.Guild)
            .Include(o => o.Users.Where(x => x.Count > 0)).ThenInclude(o => o.User!.User);
    }

    public IQueryable<GuildChannel> SetQuery(IQueryable<GuildChannel> query)
    {
        if (!string.IsNullOrEmpty(GuildId))
            query = query.Where(o => o.GuildId == GuildId);

        if (!string.IsNullOrEmpty(NameContains))
            query = query.Where(o => o.Name.Contains(NameContains));

        if (ChannelType != null)
            query = query.Where(o => o.ChannelType == ChannelType.Value);

        if (HideDeleted)
            query = query.Where(o => (o.Flags & (long)Database.Enums.ChannelFlag.Deleted) == 0);

        return query;
    }

    public IQueryable<GuildChannel> SetSort(IQueryable<GuildChannel> query)
    {
        return Sort.OrderBy switch
        {
            "Type" => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.ChannelType).ThenByDescending(o => o.Name),
                _ => query.OrderBy(o => o.ChannelType).ThenBy(o => o.Name)
            },
            "MessageCount" => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.Users.Sum(x => x.Count)).ThenByDescending(o => o.Name),
                _ => query.OrderBy(o => o.Users.Sum(x => x.Count)).ThenBy(o => o.Name)
            },
            "RolePermissions" => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.RolePermissionsCount).ThenByDescending(o => o.Name),
                _ => query.OrderBy(o => o.RolePermissionsCount)
            },
            "UserPermissions" => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.UserPermissionsCount).ThenByDescending(o => o.Name),
                _ => query.OrderBy(o => o.UserPermissionsCount).ThenBy(o => o.Name)
            },
            _ => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.Name),
                _ => query.OrderBy(o => o.Name)
            }
        };
    }

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(GuildId), GuildId },
            { nameof(NameContains), NameContains },
            { nameof(ChannelType), ChannelType?.ToString() ?? "" },
            { nameof(HideDeleted), HideDeleted.ToString() }
        };

        result.MergeDictionaryObjects(Sort, nameof(Sort));
        result.MergeDictionaryObjects(Pagination, nameof(Pagination));
        return result;
    }
}
