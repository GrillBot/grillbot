﻿using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Database.Enums;
using Quartz;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
public class OnlineUsersCleanJob : Job
{
    private GrillBotDatabaseBuilder DbFactory { get; }

    public OnlineUsersCleanJob(GrillBotDatabaseBuilder dbFactory, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        DbFactory = dbFactory;
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        await using var repository = DbFactory.CreateRepository();

        var users = await repository.User.GetOnlineUsersAsync();
        if (users.Count == 0) 
            return;

        var privateUsersOnline = new List<string>();
        var publicUsersOnline = new List<string>();

        foreach (var user in users)
        {
            if (user.HaveFlags(UserFlags.PublicAdminOnline))
                publicUsersOnline.Add(user.Username);
            if (user.HaveFlags(UserFlags.WebAdminOnline))
                privateUsersOnline.Add(user.Username);

            user.Flags &= ~(int)UserFlags.WebAdminOnline;
            user.Flags &= ~(int)UserFlags.PublicAdminOnline;
        }

        context.Result = BuildReport(privateUsersOnline, publicUsersOnline, users.Count);
        await repository.CommitAsync();
    }

    private static string BuildReport(IReadOnlyCollection<string> privateAdmin, IReadOnlyCollection<string> publicAdmin, int count)
    {
        var builder = new StringBuilder($"LoggedUsers (Count: {count})").AppendLine();

        if (privateAdmin.Count > 0)
            builder.Append("PrivateAdmin: ").AppendJoin(", ", privateAdmin).AppendLine();
        if (publicAdmin.Count > 0)
            builder.Append("PublicAdmin: ").AppendJoin(", ", publicAdmin).AppendLine();
        return builder.ToString().Trim();
    }
}
