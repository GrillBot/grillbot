using Microsoft.Extensions.Logging;

namespace GrillBot.Common.Managers;

public class InitManager
{
    private bool Initialized { get; set; }
    private readonly object locker = new();
    private ILogger<InitManager> Logger { get; }

    public InitManager(ILogger<InitManager> logger)
    {
        Logger = logger;
    }

    public void Set(bool initialized)
    {
        lock (locker)
        {
            Logger.LogInformation("Change init status (From: {Initialized}, To: {initialized})", Initialized, initialized);
            Initialized = initialized;
        }
    }

    public bool Get()
    {
        lock (locker)
        {
            return Initialized;
        }
    }
}
