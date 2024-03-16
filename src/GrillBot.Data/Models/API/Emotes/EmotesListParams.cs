using Newtonsoft.Json;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models.Pagination;
using GrillBot.Database.Models;
using GrillBot.Core.Extensions;
using GrillBot.Core.Models;

namespace GrillBot.Data.Models.API.Emotes;

public class EmotesListParams : IDictionaryObject
{
    public string? GuildId { get; set; }

    [OpenApiIgnore]
    [JsonIgnore]
    public string? UserId { get; set; }

    public RangeParams<int?>? UseCount { get; set; }
    public RangeParams<DateTime?>? FirstOccurence { get; set; }
    public RangeParams<DateTime?>? LastOccurence { get; set; }

    public bool FilterAnimated { get; set; }
    public string? EmoteName { get; set; }

    /// <summary>
    /// Available: UseCount, FirstOccurence, LastOccurence, EmoteId.
    /// Default: UseCount
    /// </summary>
    public SortParameters Sort { get; set; } = new() { OrderBy = "UseCount" };

    public PaginatedParams Pagination { get; set; } = new();

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(GuildId), GuildId },
            { nameof(FilterAnimated), FilterAnimated.ToString() },
            { nameof(EmoteName), EmoteName }
        };

        result.MergeDictionaryObjects(UseCount, nameof(UseCount));
        result.MergeDictionaryObjects(FirstOccurence, nameof(FirstOccurence));
        result.MergeDictionaryObjects(LastOccurence, nameof(LastOccurence));
        result.MergeDictionaryObjects(Sort, nameof(Sort));
        result.MergeDictionaryObjects(Pagination, nameof(Pagination));
        return result;
    }
}
