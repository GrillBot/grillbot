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

        public UnverifyCronJob(UnverifyService service, LoggingService logging)
        {
            Service = service;
            Logging = logging;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
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
