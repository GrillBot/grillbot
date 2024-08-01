using System.Collections.Generic;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Validation;
using GrillBot.Database.Models;
using GrillBot.Core.Extensions;

namespace GrillBot.Data.Models.API.Searching;

public class GetSearchingListParams : IDictionaryObject
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
