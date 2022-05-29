namespace GrillBot.Database.Services.Repository;

public abstract class RepositoryBase
{
    protected GrillBotContext Context { get; }

    protected RepositoryBase(GrillBotContext context)
    {
        Context = context;
    }
}
