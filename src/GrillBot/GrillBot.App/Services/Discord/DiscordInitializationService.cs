using Microsoft.Extensions.Logging;

namespace GrillBot.Data.Services.Discord
{
    public class DiscordInitializationService
    {
        private bool IsInitialized { get; set; }
        private readonly object _lock = new();
        private ILogger<DiscordInitializationService> Logger { get; }

        public DiscordInitializationService(ILogger<DiscordInitializationService> logger)
        {
            Logger = logger;
        }

        public void Set(bool initialized)
        {
            lock (_lock)
            {
                Logger.LogInformation($"GrillBot initialized ({initialized})");
                IsInitialized = initialized;
            }
        }

        public bool Get()
        {
            lock (_lock)
            {
                return IsInitialized;
            }
        }
    }

}
