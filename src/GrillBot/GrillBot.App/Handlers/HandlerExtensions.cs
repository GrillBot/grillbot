using GrillBot.Common.Managers.Events.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Handlers;

public static class HandlerExtensions
{
    public static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        services
            .AddSingleton<InteractionHandler>();

        services
            .AddScoped<IPresenceUpdatedEvent, PresenceUpdated.UserPresenceSynchronizationHandler>();

        services
            .AddScoped<IMessageReceivedEvent, MessageReceived.PointsMessageReceivedHandler>()
            .AddScoped<IMessageReceivedEvent, MessageReceived.ChannelMessageReceivedHandler>()
            .AddScoped<IMessageReceivedEvent, MessageReceived.UnsucessCommandHandler>()
            .AddScoped<IMessageReceivedEvent, MessageReceived.AutoReplyHandler>()
            .AddScoped<IMessageReceivedEvent, MessageReceived.EmoteMessageReceivedHandler>()
            .AddScoped<IMessageReceivedEvent, MessageReceived.EmoteChainHandler>();

        services
            .AddScoped<IReadyEvent, Ready.CommandsRegistration>()
            .AddScoped<IReadyEvent, Ready.AutoReplyReadyEvent>();

        return services;
    }
}
