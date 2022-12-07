using Discord;
using Discord.WebSocket;
using GrillBot.Common.Managers.Counters;
using GrillBot.Common.Managers.Events.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Common.Managers.Events;

public class EventManager
{
    private DiscordSocketClient DiscordClient { get; }
    private IServiceProvider ServiceProvider { get; }
    private InitManager InitManager { get; }
    private CounterManager CounterManager { get; }

    public EventManager(IDiscordClient discordClient, IServiceProvider serviceProvider, InitManager initManager, CounterManager counterManager)
    {
        DiscordClient = (DiscordSocketClient)discordClient;
        ServiceProvider = serviceProvider;
        InitManager = initManager;
        CounterManager = counterManager;

        InitEvents();
    }

    private void InitEvents()
    {
        DiscordClient.PresenceUpdated += (user, before, after) => ProcessEventAsync<IPresenceUpdatedEvent>(@event => @event.ProcessAsync(user, before, after));
        DiscordClient.MessageReceived += message => ProcessEventAsync<IMessageReceivedEvent>(@event => @event.ProcessAsync(message));
    }

    private async Task ProcessEventAsync<TInterface>(Func<TInterface, Task> processAction)
    {
        if (!InitManager.Get()) return;

        var eventName = typeof(TInterface).Name[1..];
        using (CounterManager.Create($"Events.{eventName}"))
        {
            using var scope = ServiceProvider.CreateScope();

            var services = scope.ServiceProvider.GetServices<TInterface>();
            var actions = services.Select(processAction);
            await Task.WhenAll(actions);
        }
    }
}
