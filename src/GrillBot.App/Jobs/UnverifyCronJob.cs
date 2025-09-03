using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Core.Extensions.Discord;
using GrillBot.Core.Services.Common.Executor;
using Quartz;
using UnverifyService;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
[DisallowUninitialized]
[RequireAuthentication]
public class UnverifyCronJob(
    IServiceProvider serviceProvider,
    IServiceClientExecutor<IUnverifyServiceClient> _unverifyClient
) : Job(serviceProvider)
{
    protected override async Task RunAsync(IJobExecutionContext context)
    {
        var processed = new List<string>();
        await ProcessPendingUnverifies(processed, context.CancellationToken);

        if (processed.Count > 0)
            context.Result = string.Join("\n", processed);
    }

    private async Task ProcessPendingUnverifies(List<string> log, CancellationToken cancellationToken = default)
    {
        var pending = await _unverifyClient.ExecuteRequestAsync(
            async (client, ctx) => await client.GetUnverifiesToRemoveAsync(ctx.CancellationToken),
            cancellationToken
        );

        if (pending.Count == 0)
            return;

        foreach (var unverify in pending)
        {
            var result = await _unverifyClient.ExecuteRequestAsync(
                async (client, ctx) => await client.RemoveUnverifyAsync(unverify.GuildId, unverify.UserId, false, ctx.AuthorizationToken, cancellationToken),
                cancellationToken
            );

            var guild = await DiscordClient.GetGuildAsync(unverify.GuildId, options: new() { CancelToken = cancellationToken });
            var user = await DiscordClient.GetUserAsync(unverify.UserId, options: new() { CancelToken = cancellationToken });

            log.Add($"{guild.Name} - {user?.GetDisplayName()} (Roles:{unverify.RolesToReturnCount}, Channels:{unverify.ChannelsToReturnCount})");
        }
    }
}
