using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Managers.Performance;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Common.Managers.Events;

public class EventManager
{
    private static readonly Type _readyEventType = typeof(IReadyEvent);

    private readonly EventLogManager _eventLogManager;

    private DiscordSocketClient DiscordClient { get; }
    private IServiceProvider ServiceProvider { get; }
    private InitManager InitManager { get; }
    private ICounterManager CounterManager { get; }
    private InteractionService InteractionService { get; }

    public EventManager(IDiscordClient discordClient, IServiceProvider serviceProvider, InitManager initManager, ICounterManager counterManager, InteractionService interactionService,
        EventLogManager eventLogManager)
    {
        DiscordClient = (DiscordSocketClient)discordClient;
        ServiceProvider = serviceProvider;
        InitManager = initManager;
        CounterManager = counterManager;
        InteractionService = interactionService;
        _eventLogManager = eventLogManager;

        InitEvents();
    }

    private void InitEvents()
    {
        DiscordClient.MessageReceived += message => ProcessEventAsync<IMessageReceivedEvent>(@event => @event.ProcessAsync(message), _eventLogManager.MessageReceived);
        DiscordClient.Ready += () => ProcessEventAsync<IReadyEvent>(@event => @event.ProcessAsync(), _eventLogManager.Ready);
        DiscordClient.MessageDeleted += (msg, channel) => ProcessEventAsync<IMessageDeletedEvent>(@event => @event.ProcessAsync(msg, channel), _eventLogManager.MessageDeleted);
        DiscordClient.GuildMemberUpdated += (before, after) =>
            ProcessEventAsync<IGuildMemberUpdatedEvent>(@event => @event.ProcessAsync(before.Value, after), _eventLogManager.GuildMemberUpdated);
        DiscordClient.UserJoined += user => ProcessEventAsync<IUserJoinedEvent>(@event => @event.ProcessAsync(user), _eventLogManager.UserJoined);
        DiscordClient.InviteCreated += invite => ProcessEventAsync<IInviteCreatedEvent>(@event => @event.ProcessAsync(invite), _eventLogManager.InviteCreated);
        DiscordClient.ChannelUpdated += (before, after) => ProcessEventAsync<IChannelUpdatedEvent>(@event => @event.ProcessAsync(before, after), _eventLogManager.ChannelUpdated);
        DiscordClient.GuildUpdated += (before, after) => ProcessEventAsync<IGuildUpdatedEvent>(@event => @event.ProcessAsync(before, after), _eventLogManager.GuildUpdated);
        DiscordClient.ThreadDeleted += thread => ProcessEventAsync<IThreadDeletedEvent>(@event => @event.ProcessAsync(thread.HasValue ? thread.Value : null, thread.Id), _eventLogManager.ThreadDeleted);
        DiscordClient.ReactionRemoved += (message, channel, reaction) => ProcessEventAsync<IReactionRemovedEvent>(@event => @event.ProcessAsync(message, channel, reaction), _eventLogManager.Reaction);
        DiscordClient.ReactionAdded += (message, channel, reaction) => ProcessEventAsync<IReactionAddedEvent>(@event => @event.ProcessAsync(message, channel, reaction), _eventLogManager.Reaction);
        DiscordClient.ChannelDestroyed += channel => ProcessEventAsync<IChannelDestroyedEvent>(@event => @event.ProcessAsync(channel), _eventLogManager.ChannelDeleted);
        DiscordClient.UserUpdated += (before, after) => ProcessEventAsync<IUserUpdatedEvent>(@event => @event.ProcessAsync(before, after), _eventLogManager.UserUpdated);
        DiscordClient.UserUnbanned += (user, guild) => ProcessEventAsync<IUserUnbannedEvent>(@event => @event.ProcessAsync(user, guild), _eventLogManager.UserUnbanned);
        DiscordClient.UserLeft += (guild, user) => ProcessEventAsync<IUserLeftEvent>(@event => @event.ProcessAsync(guild, user), _eventLogManager.UserLeft);
        DiscordClient.ThreadUpdated += (before, after) => ProcessEventAsync<IThreadUpdatedEvent>(@event => @event.ProcessAsync(before.HasValue ? before.Value : null, before.Id, after), _eventLogManager.ThreadUpdated);
        DiscordClient.MessageUpdated += (before, after, channel) => ProcessEventAsync<IMessageUpdatedEvent>(@event => @event.ProcessAsync(before, after, channel), _eventLogManager.MessageUpdated);
        DiscordClient.JoinedGuild += guild => ProcessEventAsync<IJoinedGuildEvent>(@event => @event.ProcessAsync(guild), _eventLogManager.JoinedGuild);
        DiscordClient.GuildAvailable += guild => ProcessEventAsync<IGuildAvailableEvent>(@event => @event.ProcessAsync(guild), _eventLogManager.GuildAvailable);
        DiscordClient.ChannelCreated += channel => ProcessEventAsync<IChannelCreatedEvent>(@event => @event.ProcessAsync(channel), _eventLogManager.ChannelCreated);
        DiscordClient.RoleDeleted += role => ProcessEventAsync<IRoleDeletedEvent>(@event => @event.ProcessAsync(role), _eventLogManager.RoleDeleted);

        InteractionService.SlashCommandExecuted +=
            (command, context, result) => ProcessEventAsync<IInteractionCommandExecutedEvent>(@event => @event.ProcessAsync(command, context, result), _eventLogManager.InteractionExecuted);
        InteractionService.ContextCommandExecuted += (command, context, result) =>
            ProcessEventAsync<IInteractionCommandExecutedEvent>(@event => @event.ProcessAsync(command, context, result), _eventLogManager.InteractionExecuted);
        InteractionService.AutocompleteCommandExecuted += (command, context, result) =>
            ProcessEventAsync<IInteractionCommandExecutedEvent>(@event => @event.ProcessAsync(command, context, result), _eventLogManager.InteractionExecuted);
        InteractionService.ComponentCommandExecuted += (command, context, result) =>
            ProcessEventAsync<IInteractionCommandExecutedEvent>(@event => @event.ProcessAsync(command, context, result), _eventLogManager.InteractionExecuted);
        InteractionService.ModalCommandExecuted += (command, context, result) =>
            ProcessEventAsync<IInteractionCommandExecutedEvent>(@event => @event.ProcessAsync(command, context, result), _eventLogManager.InteractionExecuted);
    }

    private async Task ProcessEventAsync<TInterface>(Func<TInterface, Task> processAction, Func<Task>? storeToLogAction)
    {
        var eventType = typeof(TInterface);
        var eventName = eventType.Name[1..];
        var isReadyEvent = eventType == _readyEventType;

        if (!InitManager.Get() && !isReadyEvent)
            return;

        using var scope = ServiceProvider.CreateScope();

        var services = scope.ServiceProvider.GetServices<TInterface>();
        foreach (var service in services)
        {
            var serviceName = service!.GetType().Name;

            using (CounterManager.Create($"Events.{eventName}.{serviceName}"))
            {
                await processAction(service);
            }
        }

        if (storeToLogAction is not null)
            await storeToLogAction();
        if (isReadyEvent)
            InitManager.Set(true);
    }
}
