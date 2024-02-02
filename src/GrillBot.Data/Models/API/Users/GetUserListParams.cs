using Discord;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using GrillBot.Core.Database;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Validation;
using GrillBot.Database.Models;
using GrillBot.Core.Extensions;

namespace GrillBot.Data.Models.API.Users;

public class GetUserListParams : IQueryableModel<Database.Entity.User>, IDictionaryObject
{
    /// <summary>
    /// Username.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Selected guild.
    /// </summary>
    [DiscordId]
    public string? GuildId { get; set; }

    /// <summary>
    /// Hide non-guild users. Works only if <see cref="GuildId" /> is filled. Otherwise validation error will show.
    /// </summary>
    public bool HideLeftUsers { get; set; }

    /// <summary>
    /// Selected flags from UserFlags enum.
    /// </summary>
    public long? Flags { get; set; }

    /// <summary>
    /// Select users that have stored birthday.
    /// </summary>
    public bool HaveBirthday { get; set; }

    /// <summary>
    /// Used invite code
    /// </summary>
    public string? UsedInviteCode { get; set; }

    public UserStatus? Status { get; set; }

    public SortParams Sort { get; set; } = new();
    public PaginatedParams Pagination { get; set; } = new();

    public IQueryable<Database.Entity.User> SetIncludes(IQueryable<Database.Entity.User> query)
    {
        return query
            .Include(o => o.Guilds).ThenInclude(o => o.Guild);
    }

    public IQueryable<Database.Entity.User> SetQuery(IQueryable<Database.Entity.User> query)
    {
        if (!string.IsNullOrEmpty(Username))
        {
            query = query.Where(o =>
                EF.Functions.ILike(o.Username, $"%{Username.ToLower()}%") ||
                (o.GlobalAlias != null && EF.Functions.ILike(o.GlobalAlias, $"%{Username.ToLower()}%")) ||
                o.Guilds.Any(x => x.Nickname != null && EF.Functions.ILike(x.Nickname, $"%{Username.ToLower()}%"))
            );
        }

        if (!string.IsNullOrEmpty(GuildId))
        {
            if (!HideLeftUsers)
                query = query.Where(o => o.Guilds.Any(x => x.GuildId == GuildId));
            else
                query = query.Where(o => o.Guilds.Any(x => GuildId == x.GuildId && x.IsInGuild));
        }

        if (Flags != null && Flags > 0)
            query = query.Where(o => (o.Flags & Flags) == Flags);

        if (HaveBirthday)
            query = query.Where(o => o.Birthday != null);

        if (!string.IsNullOrEmpty(UsedInviteCode))
            query = query.Where(o => o.Guilds.Any(x => !string.IsNullOrEmpty(x.UsedInviteCode) && EF.Functions.ILike(x.UsedInviteCode, $"{UsedInviteCode.ToLower()}%")));

        if (Status != null)
            query = query.Where(o => o.Status == Status);

        return query;
    }

    public IQueryable<Database.Entity.User> SetSort(IQueryable<Database.Entity.User> query)
    {
        return Sort.Descending switch
        {
            true => query.OrderByDescending(o => o.Username),
            _ => query.OrderBy(o => o.Username)
        };
    }

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(Username), Username },
            { nameof(GuildId), GuildId },
            { nameof(Flags), (Flags ?? 0).ToString() },
            { nameof(HaveBirthday), HaveBirthday.ToString() },
            { nameof(UsedInviteCode), UsedInviteCode },
            { nameof(HideLeftUsers), HideLeftUsers.ToString() }
        };

        if (Status != null)
            result[nameof(Status)] = $"{Status} ({(int)Status.Value})";
        result.MergeDictionaryObjects(Sort, nameof(Sort));
        result.MergeDictionaryObjects(Pagination, nameof(Pagination));
        return result;
    }
}
