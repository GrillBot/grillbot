using GrillBot.App.Infrastructure.Jobs;
using GrillBot.App.Jobs.Abstractions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Cooldown;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using GrillBot.Core.Services.InviteService.Models.Events;
using Quartz;
using StackExchange.Redis;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class CacheCleanerJob(
    IServiceProvider serviceProvider,
    CooldownManager _cooldownManager,
    IServer _redisServer,
    IRabbitPublisher _rabbitPublisher
) : CleanerJobBase(serviceProvider)
{
    protected override async Task RunAsync(IJobExecutionContext context)
    {
        var reportFields = new List<string>();
        await ClearExpiredCooldownsAsync(reportFields);
        await InitializeInvitesIfPossibleAsync(reportFields);

        context.Result = FormatReportFromFields(reportFields);
    }

    private async Task ClearExpiredCooldownsAsync(List<string> report)
    {
        var cleared = 0;
        var users = 0;

        foreach (var type in Enum.GetValues<CooldownType>())
        {
            await foreach (var user in DiscordClient.GetAllUsersAsync())
            {
                if (await _cooldownManager.RemoveCooldownIfExpired(user.Id.ToString(), type))
                    cleared++;
                users++;
            }
        }

        if (cleared > 0)
            report.Add($"ExpiredCooldowns: (Cleared: {cleared}, Users: {users})");
    }

    private async Task InitializeInvitesIfPossibleAsync(List<string> report)
    {
        var guilds = await DiscordClient.GetGuildsAsync();
        var payloads = new List<SynchronizeGuildInvitesPayload>();

        foreach (var guild in guilds)
        {
            var invites = await guild.GetInvitesAsync();
            if (invites.Count == 0)
                continue;

            var prefix = $"InviteMetadata-{guild.Id}-*";
            var keyExists = await _redisServer.KeysAsync(pattern: prefix, pageSize: 1).AnyAsync();
            if (keyExists)
                continue;

            report.Add($"InviteInitialization: (GuildId: {guild.Id})");
            payloads.Add(new SynchronizeGuildInvitesPayload(guild.Id.ToString(), false));
        }

        if (payloads.Count > 0)
            await _rabbitPublisher.PublishAsync(payloads);
    }
}
