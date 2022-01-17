using Discord.WebSocket;
using GrillBot.Data.Services.Discord;
using GrillBot.Data.Services.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace GrillBot.Data.Services.Unverify
{
    [DisallowConcurrentExecution]
    public class UnverifyCronJob : IJob
    {
        private UnverifyService Service { get; }
        private LoggingService Logging { get; }
        private DiscordSocketClient DiscordClient { get; }
        private DiscordInitializationService InitializationService { get; }

        public UnverifyCronJob(UnverifyService service, LoggingService logging, DiscordSocketClient discordClient,
            DiscordInitializationService initializationService)
        {
            Service = service;
            Logging = logging;
            DiscordClient = discordClient;
            InitializationService = initializationService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                if (!InitializationService.Get()) return;
                await Logging.InfoAsync(nameof(UnverifyCronJob), $"Triggered job at {DateTime.Now}");
                var pending = await Service.GetPendingUnverifiesForRemoveAsync(context.CancellationToken);

                foreach (var user in pending)
                {
                    try
                    {
                        await Service.UnverifyAutoremoveAsync(user.Item1, user.Item2, context.CancellationToken);
                    }
                    catch (Exception ex)
                    {
                        await Logging.ErrorAsync(nameof(UnverifyCronJob), $"An error occured when unverify processing for user ({user.Item1}/{user.Item2})", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                await Logging.ErrorAsync(nameof(UnverifyCronJob), "An error occured when unverify processing.", ex);
            }
        }
    }
}
