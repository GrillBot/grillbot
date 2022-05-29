namespace GrillBot.App.Services.Discord.Synchronization;

public class SynchronizationBase
{
    protected GrillBotDatabaseBuilder DbFactory { get; }

    protected SynchronizationBase(GrillBotDatabaseBuilder dbFactory)
    {
        DbFactory = dbFactory;
    }
}
