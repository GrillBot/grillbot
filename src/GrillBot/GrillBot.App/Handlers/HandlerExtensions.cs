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
            .AddScoped<IGuildMemberUpdatedEvent, GuildMemberUpdated.UserUpdatedSyncHandler>()
            .AddScoped<IGuildMemberUpdatedEvent, GuildMemberUpdated.ServerBoosterHandler>()
            .AddScoped<IGuildMemberUpdatedEvent, GuildMemberUpdated.AuditUserUpdatedHandler>()
            .AddScoped<IGuildMemberUpdatedEvent, GuildMemberUpdated.AuditUserRoleUpdatedHandler>();

        services
            .AddScoped<IMessageDeletedEvent, MessageDeleted.AuditMessageDeletedHandler>()
            .AddScoped<IMessageDeletedEvent, MessageDeleted.PointsMessageDeletedHandler>()
            .AddScoped<IMessageDeletedEvent, MessageDeleted.ChannelMessageDeletedHandler>()
            .AddScoped<IMessageDeletedEvent, MessageDeleted.EmoteMessageDeletedHandler>()
            .AddScoped<IMessageDeletedEvent, MessageDeleted.EmoteSuggestionsMessageDeletedHandler>();

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

        services
            .AddScoped<IUserJoinedEvent, UserJoined.InviteUserJoinedHandler>()
            .AddScoped<IUserJoinedEvent, UserJoined.UserJoinedSyncHandler>()
            .AddScoped<IUserJoinedEvent, UserJoined.AuditUserJoinedHandler>();

        return services;
    }
}
