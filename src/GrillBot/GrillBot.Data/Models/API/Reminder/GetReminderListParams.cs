using GrillBot.Data.Infrastructure.Validation;
using GrillBot.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using GrillBot.Common.Extensions;
using GrillBot.Common.Infrastructure;
using GrillBot.Database.Models;

namespace GrillBot.Data.Models.API.Reminder;

public class GetReminderListParams : IQueryableModel<Database.Entity.RemindMessage>, IApiObject
{
    [DiscordId]
    public string FromUserId { get; set; }

    [DiscordId]
    public string ToUserId { get; set; }

    [DiscordId]
    public string OriginalMessageId { get; set; }

    public string MessageContains { get; set; }

    public DateTime? CreatedFrom { get; set; }

    public DateTime? CreatedTo { get; set; }

    public bool OnlyWaiting { get; set; }

    /// <summary>
    /// Available: Id, FromUser, ToUser, At, Postpone
    /// Default: Id
    /// </summary>
    public SortParams Sort { get; set; } = new() { OrderBy = "Id" };

    public PaginatedParams Pagination { get; set; } = new();

    public IQueryable<Database.Entity.RemindMessage> SetIncludes(IQueryable<Database.Entity.RemindMessage> query)
    {
        return query
            .Include(o => o.FromUser)
            .Include(o => o.ToUser);
    }

    public IQueryable<Database.Entity.RemindMessage> SetQuery(IQueryable<Database.Entity.RemindMessage> query)
    {
        if (!string.IsNullOrEmpty(FromUserId))
            query = query.Where(o => o.FromUserId == FromUserId);

        if (!string.IsNullOrEmpty(ToUserId))
            query = query.Where(o => o.ToUserId == ToUserId);

        if (!string.IsNullOrEmpty(OriginalMessageId))
            query = query.Where(o => o.OriginalMessageId == OriginalMessageId);

        if (!string.IsNullOrEmpty(MessageContains))
            query = query.Where(o => o.Message.Contains(MessageContains));

        if (CreatedFrom != null)
            query = query.Where(o => o.At >= CreatedFrom.Value);

        if (CreatedTo != null)
            query = query.Where(o => o.At <= CreatedTo.Value);

        if (OnlyWaiting)
            query = query.Where(o => o.RemindMessageId == null);

        return query;
    }

    public IQueryable<Database.Entity.RemindMessage> SetSort(IQueryable<Database.Entity.RemindMessage> query)
    {
        return Sort.OrderBy switch
        {
            "FromUser" => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.FromUser.Username).ThenByDescending(o => o.FromUser.Discriminator).ThenByDescending(o => o.Id),
                _ => query.OrderBy(o => o.FromUser.Username).ThenBy(o => o.FromUser.Discriminator).ThenBy(o => o.Id)
            },
            "ToUser" => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.ToUser.Username).ThenByDescending(o => o.Id),
                _ => query.OrderBy(o => o.ToUser.Username).ThenBy(o => o.Id)
            },
            "At" => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.At).ThenByDescending(o => o.Id),
                _ => query.OrderBy(o => o.At).ThenBy(o => o.Id)
            },
            _ => Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.Id),
                _ => query.OrderBy(o => o.Id)
            }
        };
    }

    public Dictionary<string, string> SerializeForLog()
    {
        var result = new Dictionary<string, string>
        {
            { nameof(FromUserId), FromUserId },
            { nameof(ToUserId), ToUserId },
            { nameof(OriginalMessageId), OriginalMessageId },
            { nameof(MessageContains), MessageContains },
            { nameof(CreatedFrom), CreatedFrom?.ToString("o") },
            { nameof(CreatedTo), CreatedTo?.ToString("o") },
            { nameof(OnlyWaiting), OnlyWaiting.ToString() }
        };

        result.AddApiObject(Sort, nameof(Sort));
        result.AddApiObject(Pagination, nameof(Pagination));
        return result;
    }
}
