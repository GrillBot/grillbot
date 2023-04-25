using GrillBot.App.Handlers.Logging;
using GrillBot.App.Handlers.Synchronization.Database;
using GrillBot.App.Handlers.Synchronization.Services;
using GrillBot.Common.Managers.Events.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Handlers;

public static class HandlerExtensions
{
    public static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        RegisterSynchronization(services);

        services
            .AddSingleton<InteractionHandler>();

        services
            .AddScoped<IChannelCreatedEvent, ChannelCreated.AuditChannelCreatedHandler>();

        services
            .AddScoped<IChannelDestroyedEvent, ChannelDestroyed.AuditChannelDestroyedHandler>();

        services
            .AddScoped<IChannelUpdatedEvent, ChannelUpdated.AuditChannelUpdatedHandler>()
            .AddScoped<IChannelUpdatedEvent, ChannelUpdated.AuditOverwritesChangedHandler>();

        services
            .AddScoped<IGuildMemberUpdatedEvent, GuildMemberUpdated.ServerBoosterHandler>()
            .AddScoped<IGuildMemberUpdatedEvent, GuildMemberUpdated.AuditUserUpdatedHandler>()
            .AddScoped<IGuildMemberUpdatedEvent, GuildMemberUpdated.AuditUserRoleUpdatedHandler>()
            .AddScoped<IGuildMemberUpdatedEvent, GuildMemberUpdated.UserNicknameUpdatedHandler>();

        services
            .AddScoped<IGuildUpdatedEvent, GuildUpdated.AuditGuildUpdatedHandler>()
            .AddScoped<IGuildUpdatedEvent, GuildUpdated.AuditEmotesGuildUpdatedHandler>();

        services
            .AddScoped<IInteractionCommandExecutedEvent, InteractionCommandExecuted.UpdateUserLanguageHandler>()
            .AddScoped<IInteractionCommandExecutedEvent, InteractionCommandExecuted.AuditInteractionCommandHandler>();

        services
            .AddScoped<IInviteCreatedEvent, InviteCreated.InviteToCacheHandler>();

        services
            .AddScoped<WithoutAccidentRenderer>();

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
            .AddScoped<IMessageUpdatedEvent, MessageUpdated.AuditMessageUpdatedHandler>();

        services
            .AddScoped<IPresenceUpdatedEvent, PresenceUpdated.UserPresenceSynchronizationHandler>();

        services
            .AddScoped<IReactionAddedEvent, ReactionAdded.PointsReactionAddedHandler>()
            .AddScoped<IReactionAddedEvent, ReactionAdded.EmoteStatsReactionAddedHandler>();

        services
            .AddScoped<IReactionRemovedEvent, ReactionRemoved.PointsReactionRemovedHandler>()
            .AddScoped<IReactionRemovedEvent, ReactionRemoved.EmoteStatsReactionRemovedHandler>();

        services
            .AddScoped<IReadyEvent, Ready.CommandsRegistrationHandler>()
            .AddScoped<IReadyEvent, Ready.AutoReplyReadyHandler>()
            .AddScoped<IReadyEvent, Ready.InviteReadyHandler>()
            .AddScoped<IReadyEvent, Ready.UserInitSynchronizationHandler>()
            .AddScoped<IReadyEvent, Ready.ChannelInitSynchronizationHandler>();

        services
            .AddScoped<IThreadDeletedEvent, ThreadDeleted.AuditThreadDeletedHandler>();

        services
            .AddScoped<IThreadUpdatedEvent, ThreadUpdated.ForumThreadTagsUpdated>();

        services
            .AddScoped<IUserJoinedEvent, UserJoined.InviteUserJoinedHandler>()
            .AddScoped<IUserJoinedEvent, UserJoined.AuditUserJoinedHandler>();

        services
            .AddScoped<IUserLeftEvent, UserLeft.UnverifyUserLeftHandler>()
            .AddScoped<IUserLeftEvent, UserLeft.AuditUserLeftHandler>();

        services
            .AddScoped<IUserUnbannedEvent, UserUnbanned.AuditUserUnbannedHandler>();

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

        // Threads
        services
            .AddScoped<IThreadDeletedEvent, ThreadSynchronizationHandler>()
            .AddScoped<IThreadUpdatedEvent, ThreadSynchronizationHandler>();

        // Users
        services
            .AddScoped<IUserJoinedEvent, UserSynchronizationHandler>()
            .AddScoped<IGuildMemberUpdatedEvent, UserSynchronizationHandler>()
            .AddScoped<IUserUpdatedEvent, UserSynchronizationHandler>();

        // Services
        services
            .AddScoped<IUserUpdatedEvent, RubbergodServiceSynchronizationHandler>()
            .AddScoped<IUserUpdatedEvent, PointsServiceSynchronizationHandler>()
            .AddScoped<IChannelDestroyedEvent, PointsServiceSynchronizationHandler>()
            .AddScoped<IThreadDeletedEvent, PointsServiceSynchronizationHandler>();
    }
}
