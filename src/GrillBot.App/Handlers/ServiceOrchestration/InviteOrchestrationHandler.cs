using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using InviteService.Models.Events;

namespace GrillBot.App.Handlers.ServiceOrchestration;

public class InviteOrchestrationHandler(
    IRabbitPublisher _rabbitPublisher,
    IDiscordClient _discordClient
) : IInviteCreatedEvent, IReadyEvent, IUserJoinedEvent
{
    // InviteCreated
    public Task ProcessAsync(IInviteMetadata invite)
    {
        var payload = new InviteCreatedPayload(invite);
        return _rabbitPublisher.PublishAsync(payload);
    }

    // Ready
    public async Task ProcessAsync()
    {
        var guilds = await _discordClient.GetGuildsAsync();

        var payloads = guilds
            .Select(guild => guild.Id.ToString())
            .Select(id => new SynchronizeGuildInvitesPayload(id, false))
            .ToList();

        await _rabbitPublisher.PublishAsync(payloads);
    }

    // User joined
    public async Task ProcessAsync(IGuildUser user)
    {
        if (!user.IsUser())
            return;

        var payload = new UserJoinedPayload(user.GuildId.ToString(), user.Id.ToString());
        await _rabbitPublisher.PublishAsync(payload);
    }
}
