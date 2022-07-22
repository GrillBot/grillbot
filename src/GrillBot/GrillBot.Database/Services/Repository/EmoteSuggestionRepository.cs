using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class EmoteSuggestionRepository : RepositoryBase
{
    public EmoteSuggestionRepository(GrillBotContext context, CounterManager counter) : base(context, counter)
    {
    }

    private IQueryable<EmoteSuggestion> GetBaseQuery(ulong guildId, bool disableTracking = false)
    {
        var query = Context.EmoteSuggestions
            .Include(o => o.Guild)
            .Include(o => o.FromUser.User)
            .AsQueryable();

        if (disableTracking)
            query = query.AsNoTracking();

        return query
            .Where(o => o.GuildId == guildId.ToString())
            .OrderBy(o => o.Id);
    }

    public async Task<EmoteSuggestion?> FindSuggestionByMessageId(ulong guildId, ulong messageId)
    {
        using (Counter.Create("Database"))
        {
            var query = GetBaseQuery(guildId);
            return await query.FirstOrDefaultAsync(o => o.SuggestionMessageId == messageId.ToString());
        }
    }

    public async Task<List<EmoteSuggestion>> FindSuggestionsForProcessingAsync(IGuild guild)
    {
        using (Counter.Create("Database"))
        {
            var query = GetBaseQuery(guild.Id)
                .Where(o => o.ApprovedForVote != null && o.VoteMessageId == null)
                .Take(25);

            return await query.ToListAsync();
        }
    }
}
