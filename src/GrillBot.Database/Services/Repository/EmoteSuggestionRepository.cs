using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using GrillBot.Core.Database;
using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;
using GrillBot.Core.Models.Pagination;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class EmoteSuggestionRepository : RepositoryBase<GrillBotContext>
{
    public EmoteSuggestionRepository(GrillBotContext context, ICounterManager counter) : base(context, counter)
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
        using (CreateCounter())
        {
            var query = GetBaseQuery(guildId);
            return await query.FirstOrDefaultAsync(o => o.SuggestionMessageId == messageId.ToString());
        }
    }

    public async Task<List<EmoteSuggestion>> FindSuggestionsForProcessingAsync(IGuild guild)
    {
        using (CreateCounter())
        {
            var query = GetBaseQuery(guild.Id)
                .Where(o => o.ApprovedForVote != null && o.VoteMessageId == null)
                .Take(25);

            return await query.ToListAsync();
        }
    }

    public async Task<List<EmoteSuggestion>> FindSuggestionsForFinishAsync(IGuild guild)
    {
        using (CreateCounter())
        {
            var query = GetBaseQuery(guild.Id)
                .Where(o => !o.VoteFinished && o.ApprovedForVote == true && o.VoteMessageId != null && o.VoteEndsAt != null && o.VoteEndsAt.Value < DateTime.Now);

            return await query.ToListAsync();
        }
    }

    public async Task<PaginatedResponse<EmoteSuggestion>> GetSuggestionListAsync(IQueryableModel<EmoteSuggestion> model, PaginatedParams pagination)
    {
        using (CreateCounter())
        {
            var query = CreateQuery(model, true);
            return await PaginatedResponse<EmoteSuggestion>.CreateWithEntityAsync(query, pagination);
        }
    }
}
