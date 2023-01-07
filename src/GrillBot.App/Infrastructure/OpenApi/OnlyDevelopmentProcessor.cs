using System.Reflection;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace GrillBot.App.Infrastructure.OpenApi;

public class OnlyDevelopmentProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        var onlyDevelopmentAttribute = context.MethodInfo.GetCustomAttribute<OnlyDevelopmentAttribute>();
        return onlyDevelopmentAttribute == null || IsDevelopment();
    }

    private static bool IsDevelopment()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return string.Equals(environment, "Development", StringComparison.InvariantCultureIgnoreCase);
    }
}
