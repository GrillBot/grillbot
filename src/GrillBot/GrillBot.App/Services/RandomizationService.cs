using System;
using System.Collections.Concurrent;

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

        public int Next(string key) => GetOrCreateGenerator(key).Next();
        public int Next(string key, int maxValue) => GetOrCreateGenerator(key).Next(maxValue);
        public int Next(string key, int minValue, int maxValue) => GetOrCreateGenerator(key).Next(minValue, maxValue);
        public void NextBytes(string key, byte[] buffer) => GetOrCreateGenerator(key).NextBytes(buffer);
        public void NextBytes(string key, Span<byte> buffer) => GetOrCreateGenerator(key).NextBytes(buffer);
        public double NextDouble(string key) => GetOrCreateGenerator(key).NextDouble();
    }
}
