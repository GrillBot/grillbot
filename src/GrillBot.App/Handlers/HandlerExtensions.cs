using GrillBot.App.Handlers.Logging;
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
            .AddScoped<IChannelCreatedEvent, ChannelCreated.AuditChannelCreatedHandler>();

        services
            .AddScoped<IChannelDestroyedEvent, ChannelDestroyed.SyncChannelDestroyedHandler>()
            .AddScoped<IChannelDestroyedEvent, ChannelDestroyed.AuditChannelDestroyedHandler>();
            
        services
            .AddScoped<IChannelUpdatedEvent, ChannelUpdated.AuditChannelUpdatedHandler>()
            .AddScoped<IChannelUpdatedEvent, ChannelUpdated.AuditOverwritesChangedHandler>()
            .AddScoped<IChannelUpdatedEvent, ChannelUpdated.SyncChannelUpdatedHandler>();

        services
            .AddScoped<IGuildAvailableEvent, GuildAvailable.SyncGuildAvailableHandler>();

        services
            .AddScoped<IGuildMemberUpdatedEvent, GuildMemberUpdated.UserUpdatedSyncHandler>()
            .AddScoped<IGuildMemberUpdatedEvent, GuildMemberUpdated.ServerBoosterHandler>()
            .AddScoped<IGuildMemberUpdatedEvent, GuildMemberUpdated.AuditUserUpdatedHandler>()
            .AddScoped<IGuildMemberUpdatedEvent, GuildMemberUpdated.AuditUserRoleUpdatedHandler>()
            .AddScoped<IGuildMemberUpdatedEvent, GuildMemberUpdated.UserNicknameUpdatedHandler>();

        services
            .AddScoped<IGuildUpdatedEvent, GuildUpdated.SyncGuildUpdatedHandler>()
            .AddScoped<IGuildUpdatedEvent, GuildUpdated.AuditGuildUpdatedHandler>()
            .AddScoped<IGuildUpdatedEvent, GuildUpdated.AuditEmotesGuildUpdatedHandler>();

        services
            .AddScoped<IInteractionCommandExecutedEvent, InteractionCommandExecuted.UpdateUserLanguageHandler>()
            .AddScoped<IInteractionCommandExecutedEvent, InteractionCommandExecuted.AuditInteractionCommandHandler>();

        services
            .AddScoped<IInviteCreatedEvent, InviteCreated.InviteToCacheHandler>();

        services
            .AddScoped<IJoinedGuildEvent, JoinedGuild.SyncJoinedGuildHandler>()
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
            .AddScoped<IThreadDeletedEvent, ThreadDeleted.SyncThreadDeletedHandler>()
            .AddScoped<IThreadDeletedEvent, ThreadDeleted.AuditThreadDeletedHandler>();

        services
            .AddScoped<IThreadUpdatedEvent, ThreadUpdated.SyncThreadUpdatedHandler>()
            .AddScoped<IThreadUpdatedEvent, ThreadUpdated.ForumThreadTagsUpdated>();

        services
            .AddScoped<IUserJoinedEvent, UserJoined.InviteUserJoinedHandler>()
            .AddScoped<IUserJoinedEvent, UserJoined.UserJoinedSyncHandler>()
            .AddScoped<IUserJoinedEvent, UserJoined.AuditUserJoinedHandler>();

        services
            .AddScoped<IUserLeftEvent, UserLeft.UnverifyUserLeftHandler>()
            .AddScoped<IUserLeftEvent, UserLeft.AuditUserLeftHandler>();

        services
            .AddScoped<IUserUnbannedEvent, UserUnbanned.AuditUserUnbannedHandler>();

        services
            .AddScoped<IUserUpdatedEvent, UserUpdated.SyncUserInServicesHandler>()
            .AddScoped<IUserUpdatedEvent, UserUpdated.SyncUserUpdatedHandler>();

        return services;
    }
}
