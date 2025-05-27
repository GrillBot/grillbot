namespace GrillBot.App.Services.Discord;

public static class CommandsPerformanceCounter
{
    private static readonly Dictionary<string, DateTime> _runningTasks = [];
    private static readonly object _runningTasksLock = new();
    private static long _counter = 0;

    private static string CreateContextKey(IInteractionContext context)
        => $"{context.Interaction.GetType().Name}|{context.User.Id}|{context.Interaction.Id}";

    public static void StartTask(IInteractionContext context)
        => StartRunningTask(CreateContextKey(context));

    private static void StartRunningTask(string contextKey)
    {
        lock (_runningTasksLock)
        {
            if (_runningTasks.ContainsKey(contextKey))
                return;

            _runningTasks.Add(contextKey, DateTime.Now);
            _counter++;
        }
    }

    public static int TaskFinished(IInteractionContext context)
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
        lock (_runningTasksLock)
        {
            _runningTasks.Remove(contextKey, out var startAt);

            return startAt;
        }
    }

    public static bool TaskExists(IInteractionContext context)
        => TaskExists(CreateContextKey(context));

    private static bool TaskExists(string contextKey)
    {
        lock (_runningTasksLock)
        {
            return _runningTasks.ContainsKey(contextKey);
        }
    }

    public static long GetCount()
    {
        lock (_runningTasksLock)
        {
            return _counter;
        }
    }
}
