namespace GrillBot.Common.Managers.Cooldown;

public class CooldownManager
{
    private Dictionary<string, (int used, DateTime? until)> ActiveCooldowns { get; } = new();
    private readonly object _locker = new();

    public void SetCooldown(string id, CooldownType type, int maxCount, DateTime until)
    {
        lock (_locker)
        {
            var key = CreateKey(id, type);

            if (!ActiveCooldowns.ContainsKey(key))
                ActiveCooldowns.Add(key, CreateItem(1, maxCount, until));
            else
                ActiveCooldowns[key] = CreateItem(ActiveCooldowns[key].used + 1, maxCount, until);
        }
    }

    public bool IsCooldown(string id, CooldownType type, out TimeSpan? remains)
    {
        lock (_locker)
        {
            remains = null;
            if (!ActiveCooldowns.TryGetValue(CreateKey(id, type), out var data))
                return false;
            if (data.until == null || data.until.Value < DateTime.Now)
                return false;

            remains = data.until.Value - DateTime.Now;
            return true;
        }
    }

    private static string CreateKey(string id, CooldownType type)
        => $"{type}-{id}";

    private static (int used, DateTime? until) CreateItem(int used, int max, DateTime until)
        => used >= max ? (used, until) : (used, null);
}
