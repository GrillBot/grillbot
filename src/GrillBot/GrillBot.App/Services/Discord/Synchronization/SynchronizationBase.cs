namespace GrillBot.App.Services.Discord.Synchronization;

public class SynchronizationBase
{
    protected GrillBotDatabaseBuilder DatabaseBuilder { get; }

    protected SynchronizationBase(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }
}
