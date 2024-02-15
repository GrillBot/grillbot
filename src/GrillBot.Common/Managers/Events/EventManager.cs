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

    private DiscordSocketClient DiscordClient { get; }
    private IServiceProvider ServiceProvider { get; }
    private InitManager InitManager { get; }
    private ICounterManager CounterManager { get; }
    private InteractionService InteractionService { get; }
    private EventLogManager EventLogManager { get; }

    public EventManager(IDiscordClient discordClient, IServiceProvider serviceProvider, InitManager initManager, ICounterManager counterManager, InteractionService interactionService,
        EventLogManager eventLogManager)
    {
        DiscordClient = (DiscordSocketClient)discordClient;
        ServiceProvider = serviceProvider;
        InitManager = initManager;
        CounterManager = counterManager;
        InteractionService = interactionService;
        EventLogManager = eventLogManager;

        InitEvents();
    }

    private void InitEvents()
    {
        DiscordClient.MessageReceived += message => ProcessEventAsync<IMessageReceivedEvent>(@event => @event.ProcessAsync(message), EventLogManager.MessageReceived(message));
        DiscordClient.Ready += () => ProcessEventAsync<IReadyEvent>(@event => @event.ProcessAsync(), EventLogManager.Ready());
        DiscordClient.MessageDeleted += (msg, channel) => ProcessEventAsync<IMessageDeletedEvent>(@event => @event.ProcessAsync(msg, channel), EventLogManager.MessageDeleted(msg.Id, channel.Id));
        DiscordClient.GuildMemberUpdated += (before, after) =>
            ProcessEventAsync<IGuildMemberUpdatedEvent>(@event => @event.ProcessAsync(before.Value, after), EventLogManager.GuildMemberUpdated(after));
        DiscordClient.UserJoined += user => ProcessEventAsync<IUserJoinedEvent>(@event => @event.ProcessAsync(user), EventLogManager.UserJoined(user));
        DiscordClient.InviteCreated += invite => ProcessEventAsync<IInviteCreatedEvent>(@event => @event.ProcessAsync(invite), EventLogManager.InviteCreated(invite));
        DiscordClient.ChannelUpdated += (before, after) => ProcessEventAsync<IChannelUpdatedEvent>(@event => @event.ProcessAsync(before, after), EventLogManager.ChannelUpdated(after));
        DiscordClient.GuildUpdated += (before, after) => ProcessEventAsync<IGuildUpdatedEvent>(@event => @event.ProcessAsync(before, after), EventLogManager.GuildUpdated(after));
        DiscordClient.ThreadDeleted += thread =>
            ProcessEventAsync<IThreadDeletedEvent>(@event => @event.ProcessAsync(thread.HasValue ? thread.Value : null, thread.Id), EventLogManager.ThreadDeleted(thread.Id));
        DiscordClient.ReactionRemoved += (message, channel, reaction) =>
            ProcessEventAsync<IReactionRemovedEvent>(@event => @event.ProcessAsync(message, channel, reaction), EventLogManager.Reaction(reaction, false));
        DiscordClient.ReactionAdded += (message, channel, reaction) =>
            ProcessEventAsync<IReactionAddedEvent>(@event => @event.ProcessAsync(message, channel, reaction), EventLogManager.Reaction(reaction, true));
        DiscordClient.ChannelDestroyed += channel => ProcessEventAsync<IChannelDestroyedEvent>(@event => @event.ProcessAsync(channel), EventLogManager.ChannelDeleted(channel));
        DiscordClient.UserUpdated += (before, after) => ProcessEventAsync<IUserUpdatedEvent>(@event => @event.ProcessAsync(before, after), EventLogManager.UserUpdated(after));
        DiscordClient.UserUnbanned += (user, guild) => ProcessEventAsync<IUserUnbannedEvent>(@event => @event.ProcessAsync(user, guild), EventLogManager.UserUnbanned(user, guild));
        DiscordClient.UserLeft += (guild, user) => ProcessEventAsync<IUserLeftEvent>(@event => @event.ProcessAsync(guild, user), EventLogManager.UserLeft(guild, user));
        DiscordClient.ThreadUpdated += (before, after) =>
            ProcessEventAsync<IThreadUpdatedEvent>(@event => @event.ProcessAsync(before.HasValue ? before.Value : null, before.Id, after), EventLogManager.ThreadUpdated(after));
        DiscordClient.MessageUpdated += (before, after, channel) =>
            ProcessEventAsync<IMessageUpdatedEvent>(@event => @event.ProcessAsync(before, after, channel), EventLogManager.MessageUpdated(after, channel));
        DiscordClient.JoinedGuild += guild => ProcessEventAsync<IJoinedGuildEvent>(@event => @event.ProcessAsync(guild), EventLogManager.JoinedGuild(guild));
        DiscordClient.GuildAvailable += guild => ProcessEventAsync<IGuildAvailableEvent>(@event => @event.ProcessAsync(guild), EventLogManager.GuildAvailable(guild));
        DiscordClient.ChannelCreated += channel => ProcessEventAsync<IChannelCreatedEvent>(@event => @event.ProcessAsync(channel), EventLogManager.ChannelCreated(channel));
        DiscordClient.RoleDeleted += role => ProcessEventAsync<IRoleDeletedEvent>(@event => @event.ProcessAsync(role), EventLogManager.RoleDeleted(role));

        InteractionService.SlashCommandExecuted += (command, context, result) =>
            ProcessEventAsync<IInteractionCommandExecutedEvent>(@event => @event.ProcessAsync(command, context, result), EventLogManager.InteractionExecuted(command, context, result));
        InteractionService.ContextCommandExecuted += (command, context, result) =>
            ProcessEventAsync<IInteractionCommandExecutedEvent>(@event => @event.ProcessAsync(command, context, result), EventLogManager.InteractionExecuted(command, context, result));
        InteractionService.AutocompleteCommandExecuted += (command, context, result) =>
            ProcessEventAsync<IInteractionCommandExecutedEvent>(@event => @event.ProcessAsync(command, context, result), EventLogManager.InteractionExecuted(command, context, result));
        InteractionService.ComponentCommandExecuted += (command, context, result) =>
            ProcessEventAsync<IInteractionCommandExecutedEvent>(@event => @event.ProcessAsync(command, context, result), EventLogManager.InteractionExecuted(command, context, result));
        InteractionService.ModalCommandExecuted += (command, context, result) =>
            ProcessEventAsync<IInteractionCommandExecutedEvent>(@event => @event.ProcessAsync(command, context, result), EventLogManager.InteractionExecuted(command, context, result));
    }

    private async Task ProcessEventAsync<TInterface>(Func<TInterface, Task> processAction, Task? storeToLogAction)
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

        if (storeToLogAction != null)
            await storeToLogAction;
        if (isReadyEvent)
            InitManager.Set(true);
    }
}
