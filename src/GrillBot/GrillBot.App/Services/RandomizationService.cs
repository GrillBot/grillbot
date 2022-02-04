namespace GrillBot.App.Services
{
    public class RandomizationService
    {
        private ConcurrentDictionary<string, Random> Generators { get; }

        public RandomizationService()
        {
            Generators = new ConcurrentDictionary<string, Random>();
        }

        public Random GetOrCreateGenerator(string key)
        {
            if (!Generators.ContainsKey(key))
                Generators.TryAdd(key, new Random());

            return Generators[key];
        }
    }
}
