using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Validation;

namespace GrillBot.Data.Models.API.Emotes;

public class EmoteStatsUserListParams : IDictionaryObject
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
