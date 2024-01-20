using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Validation;
using GrillBot.Data.Enums;
using System;
using System.Collections.Generic;

namespace GrillBot.Data.Models.API.UserMeasures;

public class UserMeasuresParams : IDictionaryObject
{
    public UserMeasuresType? Type { get; set; }

    [DiscordId]
    public string? GuildId { get; set; }

    [DiscordId]
    public string? UserId { get; set; }

    [DiscordId]
    public string? ModeratorId { get; set; }

    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }

    public PaginatedParams Pagination { get; set; } = new();

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(GuildId), GuildId },
            { nameof(UserId), UserId },
            { nameof(ModeratorId), ModeratorId },
            { nameof(CreatedFrom), CreatedFrom?.ToString("o") },
            { nameof(CreatedTo), CreatedTo?.ToString("o") }
        };

        if (Type != null)
            result[nameof(Type)] = $"{Type} ({(int)Type.Value})";
        result.MergeDictionaryObjects(Pagination, nameof(Pagination));

        return result;
    }
}
