using Microsoft.Extensions.Logging;

namespace GrillBot.Common.Managers;

public class InitManager
{
    private bool Initialized { get; set; }
    private readonly object _locker = new();
    private ILogger<InitManager> Logger { get; }

    public InitManager(ILoggerFactory loggerFactory)
    {
        Logger = loggerFactory.CreateLogger<InitManager>();
    }

    public void Set(bool initialized)
    {
        lock (_locker)
        {
            Logger.LogInformation("Change init status (From: {Initialized}, To: {initialized})", Initialized, initialized);
            Initialized = initialized;
        }
    }

    public bool Get()
    {
        lock (_locker)
        {
            return Initialized;
        }
    }
}
