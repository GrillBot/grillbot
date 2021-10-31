using GrillBot.App.Services.Logging;
using GrillBot.Database.Enums;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.App.Services
{
    [DisallowConcurrentExecution]
    public class OnlineUsersCleanJob : IJob
    {
        private GrillBotContextFactory DbFactory { get; }
        private LoggingService Logging { get; }

        public OnlineUsersCleanJob(GrillBotContextFactory dbFactory, LoggingService logging)
        {
            DbFactory = dbFactory;
            Logging = logging;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                await Logging.InfoAsync("OnlineUsersCleanJob", $"Triggered online users clearing job at {DateTime.Now}");
                using var dbContext = DbFactory.Create();

                var usersQuery = dbContext.Users.AsQueryable()
                    .Where(o => (o.Flags & (int)UserFlags.WebAdminOnline) != 0);
                var users = await usersQuery.ToListAsync(context.CancellationToken);
                if (users.Count == 0) return;

                foreach (var user in users)
                    user.Flags &= ~(int)UserFlags.WebAdminOnline;

                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await Logging.ErrorAsync(nameof(OnlineUsersCleanJob), "An error occuret at online users clearing.", ex);
            }

        }
    }
}
