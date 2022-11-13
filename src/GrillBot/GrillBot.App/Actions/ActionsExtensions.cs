using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Actions;

public static class ActionsExtensions
{
    public static IServiceCollection AddActions(this IServiceCollection services)
    {
        return services
            .AddApiActions()
            .AddCommandsActions();
    }

    private static IServiceCollection AddApiActions(this IServiceCollection services)
    {
        // V1
        // AuditLog
        services
            .AddScoped<Api.V1.AuditLog.RemoveItem>()
            .AddScoped<Api.V1.AuditLog.GetAuditLogList>()
            .AddScoped<Api.V1.AuditLog.GetFileContent>()
            .AddScoped<Api.V1.AuditLog.CreateLogItem>();

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
            .AddScoped<Api.V1.Channel.GetChannelSimpleList>();

        // Command
        services
            .AddScoped<Api.V1.Command.CreateExplicitPermission>()
            .AddScoped<Api.V1.Command.GetCommandsList>()
            .AddScoped<Api.V1.Command.GetExplicitPermissionList>()
            .AddScoped<Api.V1.Command.GetExternalCommands>()
            .AddScoped<Api.V1.Command.RemoveExplicitPermission>()
            .AddScoped<Api.V1.Command.SetExplicitPermissionState>();

        // Emote
        services
            .AddScoped<Api.V1.Emote.GetEmoteSuggestionsList>()
            .AddScoped<Api.V1.Emote.GetStatsOfEmotes>()
            .AddScoped<Api.V1.Emote.GetSupportedEmotes>()
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
            .AddScoped<Api.V1.Invite.GetInviteList>()
            .AddScoped<Api.V1.Invite.GetMetadataCount>()
            .AddScoped<Api.V1.Invite.RefreshMetadata>();

        // Points
        services
            .AddScoped<Api.V1.Points.ComputeUserPoints>()
            .AddScoped<Api.V1.Points.GetPointsLeaderboard>()
            .AddScoped<Api.V1.Points.GetSummaries>()
            .AddScoped<Api.V1.Points.GetSummaryGraphData>()
            .AddScoped<Api.V1.Points.GetTransactionList>()
            .AddScoped<Api.V1.Points.ServiceIncrementPoints>()
            .AddScoped<Api.V1.Points.ServiceTransferPoints>();

        // PublicApiClients
        services
            .AddScoped<Api.V1.PublicApiClients.GetPublicApiMethods>()
            .AddScoped<Api.V1.PublicApiClients.CreateClient>()
            .AddScoped<Api.V1.PublicApiClients.DeleteClient>()
            .AddScoped<Api.V1.PublicApiClients.UpdateClient>()
            .AddScoped<Api.V1.PublicApiClients.GetClientsList>();

        // Reminder
        services
            .AddScoped<Api.V1.Reminder.FinishRemind>()
            .AddScoped<Api.V1.Reminder.GetReminderList>();

        // Scheduled jobs
        services
            .AddScoped<Api.V1.ScheduledJobs.GetScheduledJobs>()
            .AddScoped<Api.V1.ScheduledJobs.RunScheduledJob>();

        // Searching
        services
            .AddScoped<Api.V1.Searching.GetSearchingList>()
            .AddScoped<Api.V1.Searching.RemoveSearches>();

        // Statistics
        services
            .AddScoped<Api.V1.Statistics.GetApiStatistics>()
            .AddScoped<Api.V1.Statistics.GetAuditLogStatistics>()
            .AddScoped<Api.V1.Statistics.GetAvgTimes>()
            .AddScoped<Api.V1.Statistics.GetCommandStatistics>()
            .AddScoped<Api.V1.Statistics.GetDatabaseStatus>()
            .AddScoped<Api.V1.Statistics.GetEventStatistics>()
            .AddScoped<Api.V1.Statistics.GetUnverifyStatistics>();

        // System
        services
            .AddScoped<Api.V1.System.ChangeBotStatus>()
            .AddScoped<Api.V1.System.GetDiagnostics>()
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
            .AddScoped<Api.V1.User.UpdateUser>();

        // V2
        services
            .AddScoped<Api.V2.GetTodayBirthdayInfo>()
            .AddScoped<Api.V2.GetRubbergodUserKarma>()
            .AddScoped<Api.V2.Events.CreateScheduledEvent>()
            .AddScoped<Api.V2.Events.UpdateScheduledEvent>()
            .AddScoped<Api.V2.Events.CancelScheduledEvent>();

        return services;
    }

    private static IServiceCollection AddCommandsActions(this IServiceCollection services)
    {
        services
            .AddScoped<Commands.ImageCreator>()
            .AddScoped<Commands.Emojization>()
            .AddScoped<Commands.PointsLeaderboard>()
            .AddScoped<Commands.CleanChannelMessages>()
            .AddScoped<Commands.PurgePins>()
            .AddScoped<Commands.SendMessageToChannel>()
            .AddScoped<Commands.PermissionsCleaner>()
            .AddScoped<Commands.PermissionsReader>()
            .AddScoped<Commands.RolesReader>()
            .AddScoped<Commands.UserInfo>()
            .AddScoped<Commands.UserAccessList>()
            .AddScoped<Commands.Unverify.UnverifyList>()
            .AddScoped<Commands.GetChannelboard>()
            .AddScoped<Commands.UnsuccessCommandAttempt>();

        return services;
    }
}
