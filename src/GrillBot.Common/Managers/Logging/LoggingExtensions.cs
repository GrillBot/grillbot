using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Common.Managers.Logging;

public static class LoggingExtensions
{
    public static IServiceCollection AddLoggingServices(this IServiceCollection services)
    {
        services.AddSingleton<LoggingManager>();

        foreach (var handler in FindHandlers())
            services.AddSingleton(typeof(ILoggingHandler), handler);
        return services;
    }

    private static IEnumerable<Type> FindHandlers()
    {
        var assembly = Assembly.GetEntryAssembly();
        return FindHandlers(assembly!).DistinctBy(o => o.FullName);
    }

    private static IEnumerable<Type> FindHandlers(Assembly assembly)
    {
        var referencedAssemblies = assembly.GetReferencedAssemblies()
            .Where(o => o.Name!.StartsWith("GrillBot"))
            .Select(Assembly.Load);

        foreach (var refAssembly in referencedAssemblies)
        {
            foreach (var handler in FindHandlers(refAssembly))
                yield return handler;
        }

        foreach (var handler in assembly.GetTypes().Where(o => o.GetInterface(nameof(ILoggingHandler)) != null))
            yield return handler;
    }
}
