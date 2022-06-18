namespace GrillBot.App.Services.Discord;

public static class CommandsPerformanceCounter
{
    private static Dictionary<string, DateTime> RunningTasks { get; } = new();
    private static readonly object RunningTasksLock = new();

    private static string CreateContextKey(IInteractionContext context)
        => $"{context.Interaction.GetType().Name}|{context.User.Id}|{context.Interaction.Id}";

    private static string CreateContextKey(global::Discord.Commands.ICommandContext context)
        => $"TextBasedCommand|{context.User.Id}|{context.Message.Id}";

    public static void StartTask(IInteractionContext context)
        => StartRunningTask(CreateContextKey(context));

    public static void StartTask(global::Discord.Commands.ICommandContext context)
        => StartRunningTask(CreateContextKey(context));

    private static void StartRunningTask(string contextKey)
    {
        lock (RunningTasksLock)
        {
            if (RunningTasks.ContainsKey(contextKey))
                return;

            RunningTasks.Add(contextKey, DateTime.Now);
        }
    }

    public static int TaskFinished(IInteractionContext context)
        => RunningTaskFinished(CreateContextKey(context));

    public static int TaskFinished(global::Discord.Commands.ICommandContext context)
        => RunningTaskFinished(CreateContextKey(context));

    private static int RunningTaskFinished(string contextKey)
    {
        var startAt = RunningTaskCompleted(contextKey);

        var duration = (DateTime.Now - startAt).TotalMilliseconds;
        if (duration < 0.0) duration = 0.0;

        return Convert.ToInt32(Math.Round(duration));
    }

    private static DateTime RunningTaskCompleted(string contextKey)
    {
        lock (RunningTasksLock)
        {
            RunningTasks.Remove(contextKey, out var startAt);

            return startAt;
        }
    }

    public static bool TaskExists(IInteractionContext context)
        => TaskExists(CreateContextKey(context));

    public static bool TaskExists(global::Discord.Commands.ICommandContext context)
        => TaskExists(CreateContextKey(context));

    private static bool TaskExists(string contextKey)
    {
        lock (RunningTasksLock)
        {
            return RunningTasks.ContainsKey(contextKey);
        }
    }
}
