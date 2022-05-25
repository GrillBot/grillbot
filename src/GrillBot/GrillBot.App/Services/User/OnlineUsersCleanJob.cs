using GrillBot.App.Infrastructure.Jobs;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Logging;
using GrillBot.Common.Managers;
using GrillBot.Database.Enums;
using Quartz;

namespace GrillBot.App.Services.User;

[DisallowConcurrentExecution]
public class OnlineUsersCleanJob : Job
{
    private GrillBotContextFactory DbFactory { get; }

    public OnlineUsersCleanJob(LoggingService loggingService, AuditLogService auditLogService, IDiscordClient discordClient,
        GrillBotContextFactory dbFactory, InitManager initManager) : base(loggingService, auditLogService, discordClient, initManager)
    {
        DbFactory = dbFactory;
    }

    public override async Task RunAsync(IJobExecutionContext context)
    {
        using var dbContext = DbFactory.Create();

        var usersQuery = dbContext.Users.AsQueryable()
            .Where(o => (o.Flags & (int)UserFlags.WebAdminOnline) != 0 || (o.Flags & (int)UserFlags.PublicAdminOnline) != 0);
        var users = await usersQuery.ToListAsync(context.CancellationToken);

        if (users.Count == 0)
        {
            context.Result = "NoLoggedUsers";
            return;
        }

        foreach (var user in users)
        {
            user.Flags &= ~(int)UserFlags.WebAdminOnline;
            user.Flags &= ~(int)UserFlags.PublicAdminOnline;
        }

        context.Result = $"Users: {users.Count}";
        await dbContext.SaveChangesAsync();
    }
}
