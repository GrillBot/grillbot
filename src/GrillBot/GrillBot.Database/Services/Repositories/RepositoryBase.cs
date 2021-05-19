namespace GrillBot.Database.Services.Repositories
{
    public abstract class RepositoryBase
    {
        protected GrillBotContext Context { get; }

        protected RepositoryBase(GrillBotContext context)
        {
            Context = context;
        }
    }
}
