using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GrillBot.Core.Database;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Validation;
using GrillBot.Database.Entity;
using GrillBot.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Data.Models.API.Emotes;

public class EmoteStatsUserListParams : IQueryableModel<EmoteStatisticItem>, IDictionaryObject
{
    [DiscordId]
    [StringLength(32)]
    public string GuildId { get; set; } = null!;

    [EmoteId]
    [StringLength(255)]
    public string EmoteId { get; set; } = null!;

    /// <summary>
    /// Available: UseCount, FirstOccurence, LastOccurence, Username
    /// Default: UseCount/Descending
    /// </summary>
    public SortParameters Sort { get; set; } = new()
    {
        OrderBy = "UseCount",
        Descending = true
    };

    public PaginatedParams Pagination { get; set; } = new();

    public IQueryable<EmoteStatisticItem> SetQuery(IQueryable<EmoteStatisticItem> query)
    {
        return query.Where(o => o.EmoteId == EmoteId && o.UseCount > 0);
    }

    public IQueryable<EmoteStatisticItem> SetIncludes(IQueryable<EmoteStatisticItem> query)
    {
        return query
            .Include(o => o.Guild)
            .Include(o => o.User!.User);
    }

    public IQueryable<EmoteStatisticItem> SetSort(IQueryable<EmoteStatisticItem> query)
    {
        return Sort.OrderBy switch
        {
            "FirstOccurence" => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.FirstOccurence).ThenBy(o => o.User!.User!.Username),
                _ => query.OrderBy(o => o.FirstOccurence).ThenBy(o => o.User!.User!.Username)
            },
            "LastOccurence" => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.LastOccurence).ThenBy(o => o.User!.User!.Username),
                _ => query.OrderBy(o => o.LastOccurence).ThenBy(o => o.User!.User!.Username)
            },
            "Username" => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.User!.User!.Username),
                _ => query.OrderBy(o => o.User!.User!.Username)
            },
            _ => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.UseCount).ThenBy(o => o.User!.User!.Username),
                _ => query.OrderBy(o => o.UseCount).ThenBy(o => o.User!.User!.Username)
            }
        };
    }

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(GuildId), GuildId },
            { nameof(EmoteId), EmoteId }
        };

        result.MergeDictionaryObjects(Sort, nameof(Sort));
        result.MergeDictionaryObjects(Pagination, nameof(Pagination));
        return result;
    }
}
