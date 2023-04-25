using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Handlers.Synchronization;

public abstract class BaseSynchronizationHandler
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    protected BaseSynchronizationHandler(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    protected GrillBotRepository CreateRepository()
        => DatabaseBuilder.CreateRepository();
}
