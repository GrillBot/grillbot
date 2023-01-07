using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GrillBot.App.Services.Discord;

public class DiscordHealthCheck : IHealthCheck
{
    private DiscordSocketClient DiscordSocketClient { get; }

    public DiscordHealthCheck(DiscordSocketClient discordSocketClient)
    {
        DiscordSocketClient = discordSocketClient;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (DiscordSocketClient.ConnectionState != ConnectionState.Connected)
            return Task.FromResult(HealthCheckResult.Unhealthy($"Discord connection state is in '{DiscordSocketClient.ConnectionState}' state."));

        return Task.FromResult(DiscordSocketClient.Latency >= TimeSpan.FromSeconds(2).TotalMilliseconds
            ? HealthCheckResult.Degraded($"Discord connection is degraded. Current latency is {DiscordSocketClient.Latency}ms")
            : HealthCheckResult.Healthy());
    }
}
