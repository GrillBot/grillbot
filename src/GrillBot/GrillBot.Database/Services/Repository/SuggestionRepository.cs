using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class SuggestionRepository : RepositoryBase
{
    public SuggestionRepository(GrillBotContext context, CounterManager counter) : base(context, counter)
    {
    }

    public async Task<List<Suggestion>> GetSuggestionsAsync()
    {
        using (Counter.Create("Database"))
        {
            return await Context.Suggestions
                .OrderBy(o => o.Id)
                .ToListAsync();
        }
    } 
}
