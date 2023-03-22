using Discord;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using GrillBot.Common.Extensions;
using GrillBot.Core.Database;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Validation;
using GrillBot.Database.Models;

namespace GrillBot.Data.Models.API.Searching;

public class GetSearchingListParams : IQueryableModel<SearchItem>, IDictionaryObject
{
    [DiscordId]
    public string? UserId { get; set; }

    [DiscordId]
    public string? GuildId { get; set; }

    [DiscordId]
    public string? ChannelId { get; set; }

    public string? MessageQuery { get; set; }

    /// <summary>
    /// Available: Id, User, Guild, Channel
    /// Default: Id
    /// </summary>
    public SortParams Sort { get; set; } = new() { OrderBy = "Id" };

    public PaginatedParams Pagination { get; set; } = new();

    public IQueryable<SearchItem> SetIncludes(IQueryable<SearchItem> query)
    {
        return query
            .Include(o => o.Channel)
            .Include(o => o.Guild)
            .Include(o => o.User);
    }

    public IQueryable<SearchItem> SetQuery(IQueryable<SearchItem> query)
    {
        if (!string.IsNullOrEmpty(UserId))
            query = query.Where(o => o.UserId == UserId);

        if (!string.IsNullOrEmpty(GuildId))
            query = query.Where(o => o.GuildId == GuildId);

        if (!string.IsNullOrEmpty(ChannelId))
            query = query.Where(o => o.ChannelId == ChannelId);

        if (!string.IsNullOrEmpty(MessageQuery))
            query = query.Where(o => o.MessageContent.Contains(MessageQuery));

        return query;
    }

    public IQueryable<SearchItem> SetSort(IQueryable<SearchItem> query)
    {
        return Sort.OrderBy switch
        {
            "User" => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.User!.Username).ThenByDescending(o => o.User!.Discriminator).ThenByDescending(o => o.Id),
                _ => query.OrderBy(o => o.User!.Username).ThenBy(o => o.User!.Discriminator).ThenBy(o => o.Id)
            },
            "Guild" => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.Guild!.Name).ThenByDescending(o => o.Id),
                _ => query.OrderBy(o => o.Guild!.Name).ThenBy(o => o.Id)
            },
            "Channel" => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.Channel!.Name).ThenByDescending(o => o.Id),
                _ => query.OrderBy(o => o.Channel!.Name).ThenBy(o => o.Id)
            },
            _ => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.Id),
                _ => query.OrderBy(o => o.Id)
            }
        };
    }

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(UserId), UserId },
            { nameof(GuildId), GuildId },
            { nameof(ChannelId), ChannelId },
            { nameof(MessageQuery), MessageQuery }
        };

        result.MergeDictionaryObjects(Sort, nameof(Sort));
        result.MergeDictionaryObjects(Pagination, nameof(Pagination));
        return result;
    }
}
