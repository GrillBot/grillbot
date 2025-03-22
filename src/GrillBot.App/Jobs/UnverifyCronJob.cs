using GrillBot.App.Actions.Api.V1.Unverify;
using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Core.Extensions;
using Quartz;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class UnverifyCronJob : Job
{
    private RemoveUnverify RemoveUnverify { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public UnverifyCronJob(IServiceProvider serviceProvider, RemoveUnverify removeUnverify, GrillBotDatabaseBuilder databaseBuilder) : base(serviceProvider)
    {
        RemoveUnverify = removeUnverify;
        DatabaseBuilder = databaseBuilder;
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        var processed = new List<string>();
        using var repository = DatabaseBuilder.CreateRepository();

        var unverify = await repository.Unverify.GetFirstPendingUnverifyAsync();
        while (unverify != null)
        {
            await RemoveUnverify.ProcessAutoRemoveAsync(unverify.GuildId.ToUlong(), unverify.UserId.ToUlong());
            processed.Add($"{unverify.Guild!.Name} - {unverify.GuildUser!.DisplayName} (Roles:{unverify.Roles.Count}, Channels:{unverify.Channels.Count})");
            unverify = await repository.Unverify.GetFirstPendingUnverifyAsync();
        }

        if (processed.Count > 0)
            context.Result = string.Join("\n", processed);
    }
}
