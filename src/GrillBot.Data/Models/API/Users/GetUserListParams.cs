﻿using Discord;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using GrillBot.Common.Extensions;
using GrillBot.Core.Database;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Validation;
using GrillBot.Database.Models;

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
            query = query.Where(o => o.Username.Contains(Username));

        if (!string.IsNullOrEmpty(GuildId))
            query = query.Where(o => o.Guilds.Any(x => x.GuildId == GuildId));

        if (Flags != null)
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
            true => query.OrderByDescending(o => o.Username).ThenByDescending(o => o.Discriminator),
            _ => query.OrderBy(o => o.Username).ThenBy(o => o.Discriminator)
        };
    }

    public void FixStatus()
    {
        Status = Status switch
        {
            UserStatus.Invisible => UserStatus.Offline,
            UserStatus.AFK => UserStatus.Idle,
            _ => Status
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
        };

        if (Status != null)
            result[nameof(Status)] = $"{Status} ({(int)Status.Value})";
        result.MergeDictionaryObjects(Sort, nameof(Sort));
        result.MergeDictionaryObjects(Pagination, nameof(Pagination));
        return result;
    }
}
