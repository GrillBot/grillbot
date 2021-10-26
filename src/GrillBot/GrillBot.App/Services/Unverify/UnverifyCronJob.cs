using Discord;
using Discord.WebSocket;
using GrillBot.App.Services.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace GrillBot.App.Services.Unverify
{
    [DisallowConcurrentExecution]
    public class UnverifyCronJob : IJob
    {
        private UnverifyService Service { get; }
        private LoggingService Logging { get; }
        private DiscordSocketClient DiscordClient { get; }

        public UnverifyCronJob(UnverifyService service, LoggingService logging, DiscordSocketClient discordClient)
        {
            Service = service;
            Logging = logging;
            DiscordClient = discordClient;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                if (DiscordClient.Status != UserStatus.Online) return;
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
