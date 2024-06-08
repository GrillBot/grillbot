using System;
using System.Collections.Generic;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Validation;
using GrillBot.Database.Models;
using GrillBot.Core.Extensions;

namespace GrillBot.Data.Models.API.Reminder;

public class GetReminderListParams : IDictionaryObject
{
    [DiscordId]
    public string? FromUserId { get; set; }

    [DiscordId]
    public string? ToUserId { get; set; }

    [DiscordId]
    public string? OriginalMessageId { get; set; }

    public string? MessageContains { get; set; }

    public DateTime? CreatedFrom { get; set; }

    public DateTime? CreatedTo { get; set; }

    public bool OnlyWaiting { get; set; }

    /// <summary>
    /// Available: Id, FromUser, ToUser, At, Postpone
    /// Default: Id
    /// </summary>
    public SortParams Sort { get; set; } = new() { OrderBy = "Id" };

    public PaginatedParams Pagination { get; set; } = new();

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(FromUserId), FromUserId },
            { nameof(ToUserId), ToUserId },
            { nameof(OriginalMessageId), OriginalMessageId },
            { nameof(MessageContains), MessageContains },
            { nameof(CreatedFrom), CreatedFrom?.ToString("o") },
            { nameof(CreatedTo), CreatedTo?.ToString("o") },
            { nameof(OnlyWaiting), OnlyWaiting.ToString() }
        };

        result.MergeDictionaryObjects(Sort, nameof(Sort));
        result.MergeDictionaryObjects(Pagination, nameof(Pagination));
        return result;
    }
}
