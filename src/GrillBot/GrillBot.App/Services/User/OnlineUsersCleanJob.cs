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
    private GrillBotDatabaseBuilder DbFactory { get; }

    public OnlineUsersCleanJob(LoggingService loggingService, AuditLogWriter auditLogWriter, IDiscordClient discordClient,
        GrillBotDatabaseBuilder dbFactory, InitManager initManager) : base(loggingService, auditLogWriter, discordClient, initManager)
    {
        DbFactory = dbFactory;
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        await using var repository = DbFactory.CreateRepository();

        var users = await repository.User.GetOnlineUsersAsync();
        if (users.Count == 0) return;

        foreach (var user in users)
        {
            user.Flags &= ~(int)UserFlags.WebAdminOnline;
            user.Flags &= ~(int)UserFlags.PublicAdminOnline;
        }

        context.Result = $"LoggedUsers (Count: {users.Count}): {string.Join(", ", users.Select(o => o.Username))}";
        await repository.CommitAsync();
    }
}
