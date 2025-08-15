using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Core.Extensions.Discord;
using GrillBot.Core.Services.Common.Executor;
using Microsoft.EntityFrameworkCore;
using Quartz;
using UnverifyService;
using UnverifyService.Models.Request.Keepables;

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
        await MigrateLogItemAsync(processed, context.CancellationToken);
        await MigrateKeepableAsync(processed, context.CancellationToken);

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

    private async Task MigrateLogItemAsync(List<string> log, CancellationToken cancellationToken = default)
    {
        await using var context = ResolveService<GrillBotDatabaseBuilder>().CreateContext();

        var logItem = await context.UnverifyLogs.OrderBy(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        if (logItem is null)
            return;

        log.Add($"Migrating log item #{logItem.Id}, created {logItem.CreatedAt}.");
        var jsonData = System.Text.Json.JsonSerializer.SerializeToNode(logItem)!.AsObject();

        await _unverifyClient.ExecuteRequestAsync(
            async (client, ctx) => await client.ImportLegacyLogItemAsync(jsonData, cancellationToken),
            cancellationToken
        );

        context.UnverifyLogs.Remove(logItem);
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task MigrateKeepableAsync(List<string> log, CancellationToken cancellationToken = default)
    {
        await using var context = ResolveService<GrillBotDatabaseBuilder>().CreateContext();

        var keepables = await context.SelfunverifyKeepables
            .OrderBy(o => o.GroupName)
            .ThenBy(o => o.Name)
            .Take(10)
            .ToListAsync(cancellationToken);

        if (keepables.Count == 0)
            return;

        log.Add($"Migrating {keepables.Count} keepables.");
        var requests = keepables.ConvertAll(o => new CreateKeepableRequest
        {
            Group = o.GroupName == "_" ? "-" : o.GroupName,
            Name = o.Name,
        });

        await _unverifyClient.ExecuteRequestAsync(
            async (client, ctx) => await client.CreateKeepablesAsync(requests, cancellationToken),
            cancellationToken
        );

        context.RemoveRange(keepables);
        await context.SaveChangesAsync(cancellationToken);
    }
}
