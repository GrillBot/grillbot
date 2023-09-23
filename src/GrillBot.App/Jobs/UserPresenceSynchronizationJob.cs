﻿using GrillBot.App.Jobs.Abstractions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Core.Extensions;
using GrillBot.Database.Enums;
using GrillBot.Database.Services.Repository;
using Quartz;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
public class UserPresenceSynchronizationJob : CleanerJobBase
{
    private GrillBotDatabaseBuilder DbFactory { get; }

    public UserPresenceSynchronizationJob(GrillBotDatabaseBuilder dbFactory, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        DbFactory = dbFactory;
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        var reportFields = new List<string>();

        await using var repository = DbFactory.CreateRepository();

        await ProcessOnlineUsersAsync(repository, reportFields);
        await SynchronizeUsersStatusAsync(repository, reportFields);

        context.Result = FormatReportFromFields(reportFields);
    }

    private static async Task ProcessOnlineUsersAsync(GrillBotRepository repository, List<string> reportFields)
    {
        var users = await repository.User.GetOnlineUsersAsync();
        if (users.Count == 0)
            return;

        var privateUsersOnline = new List<string>();
        var publicUsersOnline = new List<string>();

        foreach (var user in users)
        {
            if (user.HaveFlags(UserFlags.PublicAdminOnline))
                publicUsersOnline.Add(user.GetDisplayName());
            if (user.HaveFlags(UserFlags.WebAdminOnline))
                privateUsersOnline.Add(user.GetDisplayName());

            user.Flags &= ~(int)UserFlags.WebAdminOnline;
            user.Flags &= ~(int)UserFlags.PublicAdminOnline;
        }

        await repository.CommitAsync();
        reportFields.Add(BuildReport(privateUsersOnline, publicUsersOnline, users.Count));
    }

    private async Task SynchronizeUsersStatusAsync(GrillBotRepository repository, List<string> reportFields)
    {
        var users = await repository.User.GetAllUsersAsync();

        var stateChangeStats = new Dictionary<string, int>();
        var oldStates = new Dictionary<UserStatus, int>();
        var newStates = new Dictionary<UserStatus, int>();

        foreach (var user in users)
        {
            var oldState = user.Status;
            var discordUser = await DiscordClient.FindUserAsync(user.Id.ToUlong());
            var newState = ComputeNewState(discordUser);
            if (oldState == newState) continue;

            var stateChangeKey = $"{oldState} => {newState}";
            if (!stateChangeStats.ContainsKey(stateChangeKey)) stateChangeStats.Add(stateChangeKey, 1);
            else stateChangeStats[stateChangeKey]++;

            if (!oldStates.ContainsKey(oldState)) oldStates.Add(oldState, 1);
            else oldStates[oldState]++;

            if (!newStates.ContainsKey(newState)) newStates.Add(newState, 1);
            else newStates[newState]++;

            user.Status = newState;
        }

        if (stateChangeStats.Count == 0)
            return;

        reportFields.Add(BuildReport(users.Count, stateChangeStats, oldStates, newStates));
        await repository.CommitAsync();
    }

    private static UserStatus ComputeNewState(IUser? user)
    {
        if (user?.Username.StartsWith("Deleted User", StringComparison.InvariantCultureIgnoreCase) != false)
            return UserStatus.Offline;
        return user.GetStatus();
    }

    private static string BuildReport(IReadOnlyCollection<string> privateAdmin, IReadOnlyCollection<string> publicAdmin, int count)
    {
        var builder = new StringBuilder($"LoggedUsers (Count: {count})").AppendLine();

        if (privateAdmin.Count > 0)
            builder.Append("PrivateAdmin: ").AppendJoin(", ", privateAdmin).AppendLine();
        if (publicAdmin.Count > 0)
            builder.Append("PublicAdmin: ").AppendJoin(", ", publicAdmin).AppendLine();
        return builder.ToString().Trim();
    }

    private static string BuildReport(int totalCount, Dictionary<string, int> stateChanges, Dictionary<UserStatus, int> oldStates, Dictionary<UserStatus, int> newStates)
    {
        var report = new StringBuilder("UserStatusSynchronization: (")
            .AppendLine()
            .Append(Indent).Append("Count: ").AppendLine(totalCount.ToString())
            .Append(Indent).AppendLine("StateChanges: (");

        foreach (var stateChange in stateChanges)
            report.Append(Indent).Append(Indent).Append(stateChange.Key).Append(": ").AppendLine(stateChange.Value.ToString());

        report
            .Append(Indent).AppendLine(")")
            .Append(Indent).AppendLine("OldStates: (");
        foreach (var oldState in oldStates)
            report.Append(Indent).Append(Indent).Append(oldState.Key).Append(": ").AppendLine(oldState.Value.ToString());
        report.Append(Indent).AppendLine(")");

        report
            .Append(Indent).AppendLine("NewStates: (");
        foreach (var newState in newStates)
            report.Append(Indent).Append(Indent).Append(newState.Key).Append(": ").AppendLine(newState.Value.ToString());
        report.Append(Indent).AppendLine(")");

        return report.AppendLine(")").ToString();
    }
}
