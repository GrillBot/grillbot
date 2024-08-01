using Discord.Interactions;
using GrillBot.App.Managers;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.SearchingService.Models.Events;
using GrillBot.Core.Services.SearchingService.Models.Events.Users;
using GrillBot.Database.Enums;

namespace GrillBot.App.Handlers.ServiceOrchestration;

public class SearchingOrchestrationHandler : IReadyEvent, IGuildMemberUpdatedEvent, IUserJoinedEvent, IInteractionCommandExecutedEvent
{
    private readonly IRabbitPublisher _rabbitPublisher;
    private readonly GrillBotDatabaseBuilder _databaseBuilder;
    private readonly IDiscordClient _discordClient;
    private readonly UserManager _userManager;

    public SearchingOrchestrationHandler(IRabbitPublisher rabbitPublisher, GrillBotDatabaseBuilder databaseBuilder, IDiscordClient discordClient,
        UserManager userManager)
    {
        _rabbitPublisher = rabbitPublisher;
        _databaseBuilder = databaseBuilder;
        _discordClient = discordClient;
        _userManager = userManager;
    }

    // Ready
    public async Task ProcessAsync()
    {
        var guilds = await _discordClient.GetGuildsAsync();
        var payload = new SynchronizationPayload();

        await using var repository = _databaseBuilder.CreateRepository();
        var administrators = (await repository.User.GetAdministratorsAsync()).Select(o => o.Id).ToHashSet();

        foreach (var guild in guilds)
        {
            var users = await guild.GetUsersAsync();
            foreach (var user in users)
            {
                var userId = user.Id.ToString();
                var permissions = user.GuildPermissions.ToList().Aggregate((prev, curr) => prev | curr);

                payload.Users.Add(new UserSynchronizationItem(guild.Id.ToString(), userId, administrators.Contains(userId), permissions));
            }
        }

        if (payload.Users.Count > 0)
            await _rabbitPublisher.PublishAsync(payload);
    }

    // GuildMemberUpdated
    public async Task ProcessAsync(IGuildUser? before, IGuildUser after)
    {
        if (before is not null && before.GuildPermissions.RawValue == after.GuildPermissions.RawValue)
            return;

        var isAdmin = await _userManager.CheckFlagsAsync(after, UserFlags.BotAdmin);
        var permissions = after.GuildPermissions.ToList().Aggregate((prev, curr) => prev | curr);

        var payload = new SynchronizationPayload();
        payload.Users.Add(new UserSynchronizationItem(after.GuildId.ToString(), after.Id.ToString(), isAdmin, permissions));

        await _rabbitPublisher.PublishAsync(payload);
    }

    // UserJoined
    public async Task ProcessAsync(IGuildUser user)
    {
        var permissions = user.GuildPermissions.ToList().Aggregate((prev, curr) => prev | curr);
        var payload = new SynchronizationPayload();
        payload.Users.Add(new UserSynchronizationItem(user.GuildId.ToString(), user.Id.ToString(), false, permissions));

        await _rabbitPublisher.PublishAsync(payload);
    }

    // InteractionCommandExecuted
    public async Task ProcessAsync(ICommandInfo commandInfo, IInteractionContext context, IResult result)
    {
        if (!result.IsSuccess || context.User is not IGuildUser guildUser || context.Interaction.IsDMInteraction)
            return;

        var isAdmin = await _userManager.CheckFlagsAsync(guildUser, UserFlags.BotAdmin);
        var permissions = guildUser.GuildPermissions.ToList().Aggregate((prev, curr) => prev | curr);

        var payload = new SynchronizationPayload();
        payload.Users.Add(new UserSynchronizationItem(guildUser.GuildId.ToString(), guildUser.Id.ToString(), isAdmin, permissions));

        await _rabbitPublisher.PublishAsync(payload);
    }
}
