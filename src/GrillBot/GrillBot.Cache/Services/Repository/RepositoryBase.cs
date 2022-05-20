using Microsoft.EntityFrameworkCore;

namespace GrillBot.Cache.Services.Repository;

public abstract class RepositoryBase
{
    protected GrillBotCacheContext Context { get; }

    protected RepositoryBase(GrillBotCacheContext context)
    {
        Context = context;
    }
}
