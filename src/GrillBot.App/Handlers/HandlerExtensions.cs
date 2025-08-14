using GrillBot.App.Handlers.RabbitMQ.Messages;
using GrillBot.App.Handlers.Synchronization.Database;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.RabbitMQ.V2;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Handlers;

public static class HandlerExtensions
{
    public static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        RegisterSynchronization(services);
        RegisterOrchestration(services);
        RegisterRabbit(services);

        services
            .AddSingleton<InteractionHandler>();

        services
            .AddScoped<IChannelDestroyedEvent, ChannelDestroyed.GuildConfigurationChannelDestroyedHandler>();

        services
            .AddScoped<IGuildMemberUpdatedEvent, GuildMemberUpdated.ServerBoosterHandler>();

        services
            .AddScoped<IInteractionCommandExecutedEvent, InteractionCommandExecuted.InteractionFailedCommandHandler>()
            .AddScoped<IInteractionCommandExecutedEvent, InteractionCommandExecuted.UpdateUserLanguageHandler>()
            .AddScoped<IInteractionCommandExecutedEvent, InteractionCommandExecuted.AuditInteractionCommandHandler>();

        services
            .AddScoped<Logging.WithoutAccidentRenderer>();

        services
            .AddScoped<IMessageDeletedEvent, MessageDeleted.AuditMessageDeletedHandler>()
            .AddScoped<IMessageDeletedEvent, MessageDeleted.ChannelMessageDeletedHandler>();

        services
            .AddScoped<IMessageReceivedEvent, MessageReceived.ChannelMessageReceivedHandler>()
            .AddScoped<IMessageReceivedEvent, MessageReceived.UnsucessCommandHandler>()
            .AddScoped<IMessageReceivedEvent, MessageReceived.EmoteChainHandler>()
            .AddScoped<IMessageReceivedEvent, MessageReceived.ChannelPinMessageReceivedHandler>();

        services
            .AddScoped<IReactionAddedEvent, ReactionAdded.EmoteStatsReactionAddedHandler>();

        services
            .AddScoped<IReactionRemovedEvent, ReactionRemoved.EmoteStatsReactionRemovedHandler>();

        services
            .AddScoped<IReadyEvent, Ready.GuildSynchronizationHandler>()
            .AddScoped<IReadyEvent, Ready.CommandsRegistrationHandler>()
            .AddScoped<IReadyEvent, Ready.UserInitSynchronizationHandler>()
            .AddScoped<IReadyEvent, Ready.ChannelInitSynchronizationHandler>();

        services
            .AddScoped<IRoleDeletedEvent, RoleDeleted.GuildConfigurationRoleDeletedHandler>();

        services
            .AddScoped<IThreadUpdatedEvent, ThreadUpdated.ForumThreadTagsUpdated>();

        return services;
    }

    private static void RegisterSynchronization(this IServiceCollection services)
    {
        // Channels
        services
            .AddScoped<IChannelCreatedEvent, ChannelSynchronizationHandler>()
            .AddScoped<IChannelUpdatedEvent, ChannelSynchronizationHandler>()
            .AddScoped<IChannelDestroyedEvent, ChannelSynchronizationHandler>();

        // Guild
        services
            .AddScoped<IGuildAvailableEvent, GuildSynchronizationHandler>()
            .AddScoped<IGuildUpdatedEvent, GuildSynchronizationHandler>()
            .AddScoped<IJoinedGuildEvent, GuildSynchronizationHandler>();

        // Messages
        services
            .AddScoped<IMessageUpdatedEvent, MessageSynchronizationHandler>();

        // Threads
        services
            .AddScoped<IThreadDeletedEvent, ThreadSynchronizationHandler>()
            .AddScoped<IThreadUpdatedEvent, ThreadSynchronizationHandler>();

        // Users
        services
            .AddScoped<IUserJoinedEvent, UserSynchronizationHandler>()
            .AddScoped<IGuildMemberUpdatedEvent, UserSynchronizationHandler>()
            .AddScoped<IUserUpdatedEvent, UserSynchronizationHandler>();
    }

    private static void RegisterOrchestration(IServiceCollection services)
    {
        RegisterServiceOrchestration<ServiceOrchestration.PointsOrchestrationHandler>(services);
        RegisterServiceOrchestration<ServiceOrchestration.AuditOrchestrationHandler>(services);
        RegisterServiceOrchestration<ServiceOrchestration.EmoteOrchestrationHandler>(services);
        RegisterServiceOrchestration<ServiceOrchestration.RubbergodOrchestrationHandler>(services);
        RegisterServiceOrchestration<ServiceOrchestration.SearchingOrchestrationHandler>(services);
        RegisterServiceOrchestration<ServiceOrchestration.InviteOrchestrationHandler>(services);
        RegisterServiceOrchestration<ServiceOrchestration.UserManagementOrchestrationHandler>(services);
        RegisterServiceOrchestration<ServiceOrchestration.MessageOrchestrationHandler>(services);
        RegisterServiceOrchestration<ServiceOrchestration.UnverifyOrchestrationHandler>(services);
    }

    private static void RegisterServiceOrchestration<TOrchestrationHandler>(IServiceCollection services) where TOrchestrationHandler : class
    {
        var handlerType = typeof(TOrchestrationHandler);

        foreach (var @interface in handlerType.GetInterfaces().Where(o => o.Name.EndsWith("Event")))
            services.AddScoped(@interface, handlerType);
    }

    private static void RegisterRabbit(IServiceCollection services)
    {
        services
            .AddRabbitConsumer<RabbitMQ.FileDeleteEventHandler>()
            .AddRabbitConsumer<SendMessageEventHandler>()
            .AddRabbitConsumer<RabbitMQ.ErrorNotificationEventHandler>()
            .AddRabbitConsumer<RabbitMQ.RabbitHandlerErrorHandler>()
            .AddRabbitConsumer<CreatedDiscordMessageEventHandler>()
            .AddRabbitConsumer<EditMessageEventHandler>();
    }
}
