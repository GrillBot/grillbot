using GrillBot.Database.Enums;
using System;
using System.Collections.Generic;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Validation;
using GrillBot.Database.Models;
using GrillBot.Core.Extensions;

namespace GrillBot.Data.Models.API.Unverify;

/// <summary>
/// Paginated params of unverify logs
/// </summary>
public class UnverifyLogParams : IDictionaryObject
{
    /// <summary>
    /// Selected operation.
    /// </summary>
    public UnverifyOperation? Operation { get; set; }

    /// <summary>
    /// Guild ID
    /// </summary>
    [DiscordId]
    public string? GuildId { get; set; }

    /// <summary>
    /// Who did operation. If user have lower permission, this property is ignored.
    /// </summary>
    [DiscordId]
    public string? FromUserId { get; set; }

    /// <summary>
    /// Who was target of operation. If user have lower permission, this property is ignored.
    /// </summary>
    [DiscordId]
    public string? ToUserId { get; set; }

    /// <summary>
    /// Range when operation did.
    /// </summary>
    public RangeParams<DateTime?>? Created { get; set; }

    /// <summary>
    /// Available: Operation, Guild, FromUser, ToUser, CreatedAt
    /// Default: CreatedAt.
    /// </summary>
    public SortParams Sort { get; set; } = new() { OrderBy = "CreatedAt" };

    public PaginatedParams Pagination { get; set; } = new();

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(GuildId), GuildId },
            { nameof(FromUserId), FromUserId },
            { nameof(ToUserId), ToUserId },
        };

        if (Operation != null)
            result[nameof(Operation)] = $"{Operation} ({(int)Operation.Value})";
        result.MergeDictionaryObjects(Created, nameof(Created));
        result.MergeDictionaryObjects(Sort, nameof(Sort));
        result.MergeDictionaryObjects(Pagination, nameof(Pagination));
        return result;
    }
}
