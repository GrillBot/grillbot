using Discord.Interactions;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Managers.Logging;
using GrillBot.Core.Extensions;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using GrillBot.Core.Services.Common.Exceptions;
using GrillBot.Core.Services.Common.Executor;
using StackExchange.Redis;
using UnverifyService;

namespace GrillBot.App.Handlers.ServiceOrchestration;

public class UnverifyOrchestrationHandler(
    IRabbitPublisher _rabbitPublisher,
    IServiceClientExecutor<IUnverifyServiceClient> _unverifyClient,
    IDiscordClient _discordClient,
    LoggingManager _logging
) : IUserLeftEvent, IInteractionCommandExecutedEvent, IRoleDeletedEvent, IReadyEvent
{
    // UserLeft
    public Task ProcessAsync(IGuild guild, IUser user)
    {
        var message = new UnverifyService.Models.Events.GuildUserLeftMessage
        {
            GuildId = guild.Id,
            UserId = user.Id
        };

        return _rabbitPublisher.PublishAsync(message);
    }

    // InteractionCommandExecuted
    public Task ProcessAsync(ICommandInfo commandInfo, IInteractionContext context, IResult result)
    {
        var message = new UnverifyService.Models.Events.SynchronizationMessage([
            new UnverifyService.Models.Events.UserSyncMessage
            {
                IsBot = context.User.IsBot,
                UserId = context.User.Id,
                UserLanguage = TextsManager.FixLocale(context.Interaction.UserLocale)
            }
        ]);

        return _rabbitPublisher.PublishAsync(message);
    }

    // RoleDeleted
    public async Task ProcessAsync(IRole role)
    {
        var guildInfo = await _unverifyClient.ExecuteRequestAsync(
            async (client, ctx) => await client.GetGuildInfoAsync(role.Guild.Id, ctx.CancellationToken)
        );

        if (guildInfo is null || guildInfo.MuteRoleId != role.Id.ToString())
            return;

        await _unverifyClient.ExecuteRequestAsync(
            async (client, ctx) => await client.ModifyGuildAsync(role.Guild.Id, new() { MuteRoleId = null }, ctx.CancellationToken)
        );
    }

    // Ready
    public async Task ProcessAsync()
    {
        var guilds = await _discordClient.GetGuildsAsync();

        foreach (var guild in guilds)
        {
            try
            {
                var guildInfo = await _unverifyClient.ExecuteRequestAsync(
                    async (client, ctx) => await client.GetGuildInfoAsync(guild.Id, ctx.CancellationToken)
                );

                if (string.IsNullOrEmpty(guildInfo?.MuteRoleId))
                    continue;

                var role = guild.GetRole(guildInfo.MuteRoleId.ToUlong());
                role ??= await guild.GetRoleAsync(guildInfo.MuteRoleId.ToUlong());

                if (role is not null)
                    continue;

                await _unverifyClient.ExecuteRequestAsync(
                    async (client, ctx) => await client.ModifyGuildAsync(guild.Id, new() { MuteRoleId = null }, ctx.CancellationToken)
                );
            }
            catch (Exception ex)
            {
                if (ex is ClientNotFoundException)
                    continue; // Ignore unknown guilds in the service.

                await _logging.ErrorAsync(
                    nameof(UnverifyOrchestrationHandler),
                    $"An error occured while synchronizing unverify service for guild {guild.Name}.",
                    ex
                );
            }
        }
    }
}
