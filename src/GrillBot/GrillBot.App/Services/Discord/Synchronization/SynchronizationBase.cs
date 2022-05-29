namespace GrillBot.App.Services.Discord.Synchronization;

public class SynchronizationBase
{
    protected GrillBotDatabaseFactory DbFactory { get; }

    protected SynchronizationBase(GrillBotDatabaseFactory dbFactory)
    {
        DbFactory = dbFactory;
    }
}
