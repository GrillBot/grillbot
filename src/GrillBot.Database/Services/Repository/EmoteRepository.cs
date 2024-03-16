using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class EmoteRepository : SubRepositoryBase<GrillBotContext>
{
    public EmoteRepository(GrillBotContext context, ICounterManager counter) : base(context, counter)
    {
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
            if (entity != null)
                return entity;

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

    public async Task<List<EmoteStatisticItem>> GetAllStatisticsAsync()
    {
        using (CreateCounter())
        {
            return await Context.Emotes.ToListAsync();
        }
    }

    public async Task<List<EmoteStatisticItem>> GetStatisticsOfGuildAsync(IGuild guild)
    {
        using (CreateCounter())
        {
            return await Context.Emotes
                .Where(o => o.GuildId == guild.Id.ToString())
                .ToListAsync();
        }
    }
}
