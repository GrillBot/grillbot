﻿using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.PointsService;
using GrillBot.Core.Services.RubbergodService;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Actions;

public static class ActionsExtensions
{
    public static IServiceCollection AddActions(this IServiceCollection services)
    {
        return services
            .AddServiceBridge()
            .AddApiActions()
            .AddCommandsActions();
    }

    private static IServiceCollection AddServiceBridge(this IServiceCollection services)
    {
        services.AddScoped<RabbitMQPublisherAction>();

        return services
            .AddScoped<Api.ServiceBridgeAction<IRubbergodServiceClient>>()
            .AddScoped<Api.ServiceBridgeAction<IAuditLogServiceClient>>()
            .AddScoped<Api.ServiceBridgeAction<IPointsServiceClient>>();
    }

    private static IServiceCollection AddApiActions(this IServiceCollection services)
    {
        // V1
        // AuditLog
        services
            .AddScoped<Api.V1.AuditLog.GetAuditLogList>()
            .AddScoped<Api.V1.AuditLog.CreateLogItem>()
            .AddScoped<Api.V1.AuditLog.GetAuditLogDetail>();

        // Auth
        services
            .AddScoped<Api.V1.Auth.GetRedirectLink>()
            .AddScoped<Api.V1.Auth.ProcessCallback>()
            .AddScoped<Api.V1.Auth.CreateToken>();

        // AutoReply
        services
            .AddScoped<Api.V1.AutoReply.GetAutoReplyList>()
            .AddScoped<Api.V1.AutoReply.GetAutoReplyItem>()
            .AddScoped<Api.V1.AutoReply.CreateAutoReplyItem>()
            .AddScoped<Api.V1.AutoReply.UpdateAutoReplyItem>()
            .AddScoped<Api.V1.AutoReply.RemoveAutoReplyItem>();

        // Channel
        services
            .AddScoped<Api.V1.Channel.GetChannelUsers>()
            .AddScoped<Api.V1.Channel.SendMessageToChannel>()
            .AddScoped<Api.V1.Channel.GetChannelList>()
            .AddScoped<Api.V1.Channel.ClearMessageCache>()
            .AddScoped<Api.V1.Channel.GetChannelDetail>()
            .AddScoped<Api.V1.Channel.UpdateChannel>()
            .AddScoped<Api.V1.Channel.GetChannelboard>()
            .AddScoped<Api.V1.Channel.SimpleList.GetChannelSimpleList>()
            .AddScoped<Api.V1.Channel.SimpleList.GetChannelSimpleListWithPins>()
            .AddScoped<Api.V1.Channel.GetPins>()
            .AddScoped<Api.V1.Channel.GetPinsWithAttachments>();

        // Dashboard
        services
            .AddScoped<Api.V1.Dashboard.GetActiveOperations>()
            .AddScoped<Api.V1.Dashboard.GetCommonInfo>()
            .AddScoped<Api.V1.Dashboard.GetUserMeasuresDashboard>()
            .AddScoped<Api.V1.Dashboard.GetOperationStats>()
            .AddScoped<Api.V1.Dashboard.GetServicesList>();

        // Emote
        services
            .AddScoped<Api.V1.Emote.GetStatOfEmote>()
            .AddScoped<Api.V1.Emote.GetStatsOfEmotes>()
            .AddScoped<Api.V1.Emote.GetSupportedEmotes>()
            .AddScoped<Api.V1.Emote.GetUserStatisticsOfEmote>()
            .AddScoped<Api.V1.Emote.MergeStats>()
            .AddScoped<Api.V1.Emote.RemoveStats>();

        // Guild
        services
            .AddScoped<Api.V1.Guild.GetAvailableGuilds>()
            .AddScoped<Api.V1.Guild.GetGuildDetail>()
            .AddScoped<Api.V1.Guild.GetGuildList>()
            .AddScoped<Api.V1.Guild.GetRoles>()
            .AddScoped<Api.V1.Guild.UpdateGuild>();

        // Invite
        services
            .AddScoped<Api.V1.Invite.DeleteInvite>()
            .AddScoped<Api.V1.Invite.GetInviteList>()
            .AddScoped<Api.V1.Invite.GetMetadataCount>()
            .AddScoped<Api.V1.Invite.RefreshMetadata>();

        // Points
        services
            .AddScoped<Api.V1.Points.ComputeUserPoints>()
            .AddScoped<Api.V1.Points.GetPointsLeaderboard>()
            .AddScoped<Api.V1.Points.GetTransactionList>()
            .AddScoped<Api.V1.Points.ServiceIncrementPoints>()
            .AddScoped<Api.V1.Points.ServiceTransferPoints>()
            .AddScoped<Api.V1.Points.GetUserList>();

        // PublicApiClients
        services
            .AddScoped<Api.V1.PublicApiClients.GetPublicApiMethods>()
            .AddScoped<Api.V1.PublicApiClients.CreateClient>()
            .AddScoped<Api.V1.PublicApiClients.DeleteClient>()
            .AddScoped<Api.V1.PublicApiClients.GetClient>()
            .AddScoped<Api.V1.PublicApiClients.UpdateClient>()
            .AddScoped<Api.V1.PublicApiClients.GetClientsList>();

        // Reminder
        services
            .AddScoped<Api.V1.Reminder.FinishRemind>()
            .AddScoped<Api.V1.Reminder.GetReminderList>();

        // Scheduled jobs
        services
            .AddScoped<Api.V1.ScheduledJobs.GetScheduledJobs>()
            .AddScoped<Api.V1.ScheduledJobs.RunScheduledJob>()
            .AddScoped<Api.V1.ScheduledJobs.UpdateJob>();

        // Searching
        services
            .AddScoped<Api.V1.Searching.GetSearchingList>()
            .AddScoped<Api.V1.Searching.RemoveSearches>();

        // Services
        services
            .AddScoped<Api.V1.Services.GetServiceInfo>();

        // Statistics
        services
            .AddScoped<Api.V1.Statistics.GetApiUserStatistics>()
            .AddScoped<Api.V1.Statistics.GetDatabaseStatus>()
            .AddScoped<Api.V1.Statistics.GetEventStatistics>()
            .AddScoped<Api.V1.Statistics.GetUnverifyStatistics>()
            .AddScoped<Api.V1.Statistics.GetOperationStats>()
            .AddScoped<Api.V1.Statistics.GetUserCommandStatistics>();

        // System
        services
            .AddScoped<Api.V1.System.ChangeBotStatus>()
            .AddScoped<Api.V1.System.GetEventLog>();

        // Unverify
        services
            .AddScoped<Api.V1.Unverify.AddKeepables>()
            .AddScoped<Api.V1.Unverify.GetCurrentUnverifies>()
            .AddScoped<Api.V1.Unverify.GetKeepablesList>()
            .AddScoped<Api.V1.Unverify.GetLogs>()
            .AddScoped<Api.V1.Unverify.KeepableExists>()
            .AddScoped<Api.V1.Unverify.RecoverState>()
            .AddScoped<Api.V1.Unverify.RemoveKeepables>()
            .AddScoped<Api.V1.Unverify.RemoveUnverify>()
            .AddScoped<Api.V1.Unverify.UpdateUnverify>();

        // User
        services
            .AddScoped<Api.V1.User.GetAvailableUsers>()
            .AddScoped<Api.V1.User.GetUserDetail>()
            .AddScoped<Api.V1.User.GetUserList>()
            .AddScoped<Api.V1.User.UpdateUser>()
            .AddScoped<Api.V1.User.Hearthbeat>();

        services
            .AddScoped<Api.V1.UserMeasures.CreateUserMeasuresWarning>()
            .AddScoped<Api.V1.UserMeasures.GetUserMeasuresList>();

        // V2
        services
            .AddScoped<Api.V2.GetTodayBirthdayInfo>()
            .AddScoped<Api.V2.AuditLog.CreateAuditLogMessageAction>()
            .AddScoped<Api.V2.User.GetRubbergodUserKarma>()
            .AddScoped<Api.V2.User.GetGuildUserInfo>()
            .AddScoped<Api.V2.User.CreateUserMeasuresTimeout>();

        return services;
    }

    private static IServiceCollection AddCommandsActions(this IServiceCollection services)
    {
        // Birthday
        services
            .AddScoped<Commands.Birthday.AddBirthday>()
            .AddScoped<Commands.Birthday.HaveBirthday>()
            .AddScoped<Commands.Birthday.RemoveBirthday>();

        // Emotes
        services
            .AddScoped<Commands.Emotes.EmoteInfo>()
            .AddScoped<Commands.Emotes.GetEmotesList>();

        // Guild
        services
            .AddScoped<Commands.Guild.GuildInfo>();

        // Images
        services
            .AddScoped<Commands.Images.ImageCreator>();

        // Points
        services
            .AddScoped<Commands.Points.Chart.PointsChart>()
            .AddScoped<Commands.Points.Chart.UserChartBuilder>()
            .AddScoped<Commands.Points.Chart.GuildChartBuilder>()
            .AddScoped<Commands.Points.PointsLeaderboard>()
            .AddScoped<Commands.Points.PointsImage>();

        // Reminder
        services
            .AddScoped<Commands.Reminder.CreateRemind>()
            .AddScoped<Commands.Reminder.CopyRemind>()
            .AddScoped<Commands.Reminder.GetSuggestions>()
            .AddScoped<Commands.Reminder.GetReminderList>();

        // Searching
        services
            .AddScoped<Commands.Searching.CreateSearch>()
            .AddScoped<Commands.Searching.GetSearchingList>()
            .AddScoped<Commands.Searching.GetSuggestions>()
            .AddScoped<Commands.Searching.RemoveSearch>();

        // Unverify
        services
            .AddScoped<Commands.Unverify.SelfUnverifyKeepables>()
            .AddScoped<Commands.Unverify.SetUnverify>()
            .AddScoped<Commands.Unverify.UnverifyList>();

        services
            .AddScoped<Commands.Permissions.PermissionsCleaner>()
            .AddScoped<Commands.Permissions.PermissionSetter>();

        services
            .AddScoped<Commands.BotInfo>()
            .AddScoped<Commands.ChannelInfo>()
            .AddScoped<Commands.CleanChannelMessages>()
            .AddScoped<Commands.DuckInfo>()
            .AddScoped<Commands.Mock>()
            .AddScoped<Commands.Emojization>()
            .AddScoped<Commands.PurgePins>()
            .AddScoped<Commands.SendMessageToChannel>()
            .AddScoped<Commands.SolveExpression>()
            .AddScoped<Commands.RolesReader>()
            .AddScoped<Commands.UserInfo>()
            .AddScoped<Commands.UserAccessList>()
            .AddScoped<Commands.GetChannelboard>();

        return services;
    }
}
