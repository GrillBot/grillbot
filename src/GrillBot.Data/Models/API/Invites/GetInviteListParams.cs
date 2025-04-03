using System;
using System.Collections.Generic;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Validation;
using GrillBot.Database.Models;
using GrillBot.Core.Extensions;

namespace GrillBot.Data.Models.API.Invites;

public class GetInviteListParams : IDictionaryObject
{
    [DiscordId]
    public string? GuildId { get; set; }

    [DiscordId]
    public string? CreatorId { get; set; }

    public string? Code { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }

    public bool ShowUnused { get; set; }

    /// <summary>
    /// Available: Code, CreatedAt, Creator, UseCount.
    /// Default: Code
    /// </summary>
    public SortParams Sort { get; set; } = new() { OrderBy = "Code" };

    public PaginatedParams Pagination { get; set; } = new();

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(GuildId), GuildId },
            { nameof(CreatorId), CreatorId },
            { nameof(Code), Code },
            { nameof(CreatedFrom), CreatedFrom?.ToString("o") },
            { nameof(CreatedTo), CreatedTo?.ToString("o") },
            { nameof(ShowUnused), ShowUnused.ToString() }
        };

        result.MergeDictionaryObjects(Sort, nameof(Sort));
        result.MergeDictionaryObjects(Pagination, nameof(Pagination));
        return result;
    }
}
