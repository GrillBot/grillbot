using GrillBot.Cache.Entity;
using GrillBot.Common.Managers.Counters;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GrillBot.Cache.Services.Repository;

public class EmoteSuggestionRepository : RepositoryBase
{
    public EmoteSuggestionRepository(GrillBotCacheContext context, CounterManager counter) : base(context, counter)
    {
    }

    public async Task PurgeExpiredAsync(int expirationHours)
    {
        if (IsInMemory) return;

        using (CreateCounter())
        {
            var validLimit = DateTime.Now.AddHours(-expirationHours);
            await Context.Database.ExecuteSqlRawAsync("DELETE FROM public.\"EmoteSuggestions\" WHERE \"CreatedAt\" < @validLimit", new NpgsqlParameter("@validLimit", validLimit));
        }
    }

    public async Task<EmoteSuggestionMetadata?> FindByIdAsync(string id)
    {
        using (CreateCounter())
        {
            return await Context.EmoteSuggestions.AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);
        }
    }
}
