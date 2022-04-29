namespace GrillBot.App.Services.Discord.Synchronization;

public class SynchronizationBase
{
    protected GrillBotContextFactory DbFactory { get; }

    protected SynchronizationBase(GrillBotContextFactory dbFactory)
    {
        DbFactory = dbFactory;
    }
}
