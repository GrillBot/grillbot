using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.TestHelpers;

[ExcludeFromCodeCoverage]
public static class CacheHelper
{
    public static IMemoryCache CreateMemoryCache()
    {
        var options = new MemoryCacheOptions();
        return new MemoryCache(options);
    }
}
