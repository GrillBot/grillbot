using Cronos;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace GrillBot.App.Services.CronJobs
{
    public abstract class CronJobTask : IHostedService, IDisposable
    {
        private System.Timers.Timer Timer { get; set; }
        private CronExpression Expression { get; }

        protected CronJobTask(string cronExpression)
        {
            Expression = CronExpression.Parse(cronExpression);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return ScheduleJobAsync(cancellationToken);
        }

        protected virtual async Task ScheduleJobAsync(CancellationToken cancellationToken)
        {
            var next = Expression.GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo.Local);
            if (next.HasValue)
            {
                var delay = next.Value - DateTimeOffset.Now;
                if (delay.TotalMilliseconds <= 0)   // prevent non-positive values from being passed into Timer
                    await ScheduleJobAsync(cancellationToken);

                Timer = new System.Timers.Timer(delay.TotalMilliseconds);
                Timer.Elapsed += async (sender, args) =>
                {
                    Timer.Dispose();  // reset and dispose timer
                    Timer = null;

                    if (!cancellationToken.IsCancellationRequested)
                        await DoWorkAsync(cancellationToken);

                    if (!cancellationToken.IsCancellationRequested)
                        await ScheduleJobAsync(cancellationToken);    // reschedule next
                };

                Timer.Start();
            }

            await Task.CompletedTask;
        }

        public abstract Task DoWorkAsync(CancellationToken cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Timer?.Stop();
            return Task.CompletedTask;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                Timer?.Dispose();

            Timer = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
