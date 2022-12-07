﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;
using GrillBot.Database.Models.Emotes;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class EmoteRepository : RepositoryBase
{
    public EmoteRepository(GrillBotContext context, CounterManager counter) : base(context, counter)
    {
    }

    public async Task<List<EmoteStatItem>> GetEmoteStatisticsDataAsync(IQueryableModel<EmoteStatisticItem> model,
        IEnumerable<string> emoteIds, bool unsupported)
    {
        using (CreateCounter())
        {
            var query = CreateQuery(model, true);
            query = unsupported ? query.Where(o => !emoteIds.Contains(o.EmoteId)) : query.Where(o => emoteIds.Contains(o.EmoteId));

            var grouped = CreateGroupingQuery(query);
            return await grouped.ToListAsync();
        }
    }

    public async Task<List<EmoteStatisticItem>> FindStatisticsByEmoteIdAsync(string emoteId)
    {
        using (CreateCounter())
        {
            return await Context.Emotes
                .Where(o => o.EmoteId == emoteId)
                .ToListAsync();
        }
    }

    public async Task<EmoteStatItem?> GetStatisticsOfEmoteAsync(IEmote emote)
    {
        using (CreateCounter())
        {
            var baseQuery = Context.Emotes.AsNoTracking()
                .Where(o => o.UseCount > 0);

            var query = CreateGroupingQuery(baseQuery);
            return await query.FirstOrDefaultAsync(o => o.EmoteId == emote.ToString());
        }
    }

    private static IQueryable<EmoteStatItem> CreateGroupingQuery(IQueryable<EmoteStatisticItem> query)
    {
        return query
            .GroupBy(o => o.EmoteId)
            .Select(o => new EmoteStatItem
            {
                EmoteId = o.Key,
                FirstOccurence = o.Min(x => x.FirstOccurence),
                LastOccurence = o.Max(x => x.LastOccurence),
                UseCount = o.Sum(x => x.UseCount),
                UsedUsersCount = o.Count(),
                GuildId = o.Min(x => x.GuildId)!
            });
    }

    public async Task<List<EmoteStatisticItem>> GetTopUsersOfUsage(IEmote emote, int count)
    {
        using (CreateCounter())
        {
            var query = Context.Emotes.AsNoTracking()
                .Where(o => o.UseCount > 0 && o.EmoteId == emote.ToString())
                .OrderByDescending(o => o.UseCount)
                .ThenByDescending(o => o.LastOccurence)
                .Take(count);

            return await query.ToListAsync();
        }
    }

    public async Task<EmoteStatisticItem?> FindStatisticAsync(IEmote emote, IUser user, IGuild guild)
    {
        using (CreateCounter())
        {
            return await Context.Emotes
                .FirstOrDefaultAsync(o => o.EmoteId == emote.ToString() && o.UserId == user.Id.ToString() && o.GuildId == guild.Id.ToString());
        }
    }

    public async Task<EmoteStatisticItem> GetOrCreateStatisticAsync(IEmote emote, IUser user, IGuild guild)
    {
        using (CreateCounter())
        {
            var entity = await FindStatisticAsync(emote, user, guild);
            if (entity != null) return entity;

            entity = new EmoteStatisticItem
            {
                GuildId = guild.Id.ToString(),
                EmoteId = emote.ToString()!,
                FirstOccurence = DateTime.Now,
                UserId = user.Id.ToString()
            };
            await Context.AddAsync(entity);
            return entity;
        }
    }
}
