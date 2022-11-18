using GrillBot.App.Infrastructure.Jobs;
using Quartz;

namespace GrillBot.App.Services.Unverify;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class UnverifyCronJob : Job
{
    private UnverifyService UnverifyService { get; }

    public UnverifyCronJob(UnverifyService unverifyService, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        UnverifyService = unverifyService;
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        var pending = await UnverifyService.GetPendingUnverifiesForRemoveAsync();

        foreach (var (guildId, userId) in pending)
            await UnverifyService.UnverifyAutoremoveAsync(guildId, userId);

        if (pending.Count > 0)
            context.Result = $"Processed: {string.Join(", ", pending.Select(o => $"{o.Item1}/{o.Item2}"))}";
    }
}
