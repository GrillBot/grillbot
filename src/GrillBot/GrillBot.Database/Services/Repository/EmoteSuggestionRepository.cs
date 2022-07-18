using System.Linq;
using System.Threading.Tasks;
using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class EmoteSuggestionRepository : RepositoryBase
{
    public EmoteSuggestionRepository(GrillBotContext context, CounterManager counter) : base(context, counter)
    {
    }

    public async Task<EmoteSuggestion?> FindSuggestionByMessageId(ulong messageId)
    {
        using (Counter.Create("Database"))
        {
            var query = Context.EmoteSuggestions
                .Include(o => o.Guild)
                .Include(o => o.FromUser.User)
                .AsQueryable();

            return await query.FirstOrDefaultAsync(o => o.SuggestionMessageId == messageId.ToString());
        }
    }
}
