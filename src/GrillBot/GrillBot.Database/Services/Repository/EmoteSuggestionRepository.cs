using GrillBot.Common.Managers.Counters;

namespace GrillBot.Database.Services.Repository;

public class EmoteSuggestionRepository : RepositoryBase
{
    public EmoteSuggestionRepository(GrillBotContext context, CounterManager counter) : base(context, counter)
    {
    }
}
