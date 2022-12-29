namespace GrillBot.Common.Managers;

public class RandomizationManager
{
    private readonly object _locker = new();
    private Dictionary<string, Random> Generators { get; } = new();

    private Random GetOrCreate(string key)
    {
        lock (_locker)
        {
            if (!Generators.ContainsKey(key))
                Generators.Add(key, new Random());

            return Generators[key];
        }
    }

    public int GetNext(string key, int min, int max)
        => GetOrCreate(key).Next(min, max);

    public int GetNext(string key, int max)
        => GetOrCreate(key).Next(max);
}
