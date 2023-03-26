using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;

namespace GrillBot.Database.Services.Repository;

public class PointsRepository : RepositoryBase<GrillBotContext>
{
    public PointsRepository(GrillBotContext context, ICounterManager counter) : base(context, counter)
    {
    }
}
