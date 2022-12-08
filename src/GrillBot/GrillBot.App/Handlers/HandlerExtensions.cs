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
            .AddScoped<IMessageDeleted, MessageDeleted.AuditMessageDeleted>()
            .AddScoped<IMessageDeleted, MessageDeleted.PointsMessageDeletedHandler>()
            .AddScoped<IMessageDeleted, MessageDeleted.ChannelMessageDeletedHandler>()
            .AddScoped<IMessageDeleted, MessageDeleted.EmoteMessageDeletedHandler>()
            .AddScoped<IMessageDeleted, MessageDeleted.EmoteSuggestionsMessageDeletedHandler>();

        services
            .AddScoped<IMessageReceivedEvent, MessageReceived.PointsMessageReceivedHandler>()
            .AddScoped<IMessageReceivedEvent, MessageReceived.ChannelMessageReceivedHandler>()
            .AddScoped<IMessageReceivedEvent, MessageReceived.UnsucessCommandHandler>()
            .AddScoped<IMessageReceivedEvent, MessageReceived.AutoReplyHandler>()
            .AddScoped<IMessageReceivedEvent, MessageReceived.EmoteMessageReceivedHandler>()
            .AddScoped<IMessageReceivedEvent, MessageReceived.EmoteChainHandler>();

        services
            .AddScoped<IPresenceUpdatedEvent, PresenceUpdated.UserPresenceSynchronizationHandler>();

        services
            .AddScoped<IReadyEvent, Ready.CommandsRegistrationHandler>()
            .AddScoped<IReadyEvent, Ready.AutoReplyReadyHandler>()
            .AddScoped<IReadyEvent, Ready.InviteReadyHandler>()
            .AddScoped<IReadyEvent, Ready.UserInitSynchronizationHandler>()
            .AddScoped<IReadyEvent, Ready.ChannelInitSynchronizationHandler>();

        return services;
    }
}
