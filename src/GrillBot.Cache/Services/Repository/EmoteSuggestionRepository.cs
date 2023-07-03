using GrillBot.Cache.Entity;
using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GrillBot.Cache.Services.Repository;

public class EmoteSuggestionRepository : SubRepositoryBase<GrillBotCacheContext>
{
    public EmoteSuggestionRepository(GrillBotCacheContext context, ICounterManager counter) : base(context, counter)
    {
    }

    public async Task<int> PurgeExpiredAsync(int expirationHours)
    {
        using (CreateCounter())
        {
            var validLimit = DateTime.Now.AddHours(-expirationHours);
            return await Context.Database.ExecuteSqlRawAsync("DELETE FROM public.\"EmoteSuggestions\" WHERE \"CreatedAt\" < @validLimit", new NpgsqlParameter("@validLimit", validLimit));
        }
    }

    public async Task<EmoteSuggestionMetadata?> FindByIdAsync(string id, int validHours)
    {
        using (CreateCounter())
        {
            var validLimit = DateTime.Now.AddHours(-validHours);
            return await Context.EmoteSuggestions.AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id && o.CreatedAt > validLimit);
        }
    }
}
