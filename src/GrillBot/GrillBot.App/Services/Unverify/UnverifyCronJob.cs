using GrillBot.App.Actions.Api.V1.Unverify;
using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace GrillBot.App.Services.Unverify;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class UnverifyCronJob : Job
{
    private RemoveUnverify RemoveUnverify { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public UnverifyCronJob(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        RemoveUnverify = serviceProvider.GetRequiredService<RemoveUnverify>();
        DatabaseBuilder = serviceProvider.GetRequiredService<GrillBotDatabaseBuilder>();
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        var processed = new List<string>();
        await using var repository = DatabaseBuilder.CreateRepository();

        var unverify = await repository.Unverify.GetFirstPendingUnverifyAsync();
        while (unverify != null)
        {
            await RemoveUnverify.ProcessAutoRemoveAsync(unverify.GuildId.ToUlong(), unverify.UserId.ToUlong());
            processed.Add($"{unverify.Guild!.Name} - {unverify.GuildUser!.FullName()} (Roles:{unverify.Roles.Count}, Channels:{unverify.Channels.Count})");
            unverify = await repository.Unverify.GetFirstPendingUnverifyAsync();
        }

        if (processed.Count > 0)
            context.Result = string.Join("\n", processed);
    }
}
