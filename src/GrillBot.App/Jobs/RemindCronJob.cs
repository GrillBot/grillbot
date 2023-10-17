using GrillBot.App.Actions.Api.V1.Reminder;
using GrillBot.App.Infrastructure.Jobs;
using Quartz;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class RemindCronJob : Job
{
    private FinishRemind FinishRemind { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public RemindCronJob(FinishRemind finishRemind, GrillBotDatabaseBuilder databaseBuilder, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        DatabaseBuilder = databaseBuilder;

        FinishRemind = finishRemind;
        FinishRemind.UpdateContext("en-US", DiscordClient.CurrentUser);
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        var reminders = await GetIdsForProcessAsync();

        var result = new Dictionary<long, string>();
        foreach (var id in reminders)
        {
            try
            {
                await FinishRemind.ProcessAsync(id, true, false);
                result.Add(id, CreateReportMessage());
                FinishRemind.ResetState();
            }
            catch (Exception ex)
            {
                result.Add(id, CreateReportMessage(ex));
                await LoggingManager.ErrorAsync(nameof(RemindCronJob), $"An error occured while processing remind #{id}", ex);
            }
        }

        if (result.Count > 0)
        {
            var resultBuilder = new StringBuilder($"Processed reminders ({reminders.Count}):").AppendLine()
                .AppendJoin("\n", result.Select(o => $"{o.Key}: {o.Value}"));

            context.Result = resultBuilder.ToString();
        }
    }

    private async Task<List<long>> GetIdsForProcessAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        return await repository.Remind.GetRemindIdsForProcessAsync();
    }

    private string CreateReportMessage(Exception? exception = null)
    {
        var result = new List<string>();

        if (FinishRemind.Remind is not null)
        {
            result.Add($"FromUser: {FinishRemind.Remind.FromUser!.GetDisplayName()}");
            result.Add($"ToUser: {FinishRemind.Remind.ToUser!.GetDisplayName()}");
            result.Add($"MessageLength: {FinishRemind.Remind.Message.Length}");
            result.Add($"Language: {FinishRemind.Remind.Language}");
        }

        if (exception is not null)
        {
            result.Add($"Exception: {exception.Message}");
        }
        else
        {
            if (string.IsNullOrEmpty(FinishRemind.ErrorMessage))
                result.Add("Finished successfully");
            else
                result.Add($"Error: {FinishRemind.ErrorMessage}");
        }

        return string.Join(", ", result);
    }
}
