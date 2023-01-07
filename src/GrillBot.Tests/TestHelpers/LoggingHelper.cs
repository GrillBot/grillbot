using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.TestHelpers;

[ExcludeFromCodeCoverage]
public static class LoggingHelper
{
    public static ILoggerFactory CreateLoggerFactory()
    {
        return NullLoggerFactory.Instance;
    }
}
