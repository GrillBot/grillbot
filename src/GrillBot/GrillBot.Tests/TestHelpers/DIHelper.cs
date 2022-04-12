using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.TestHelpers;

[ExcludeFromCodeCoverage]
public static class DIHelper
{
    public static IServiceProvider CreateEmptyProvider()
    {
        return new ServiceCollection().BuildServiceProvider();
    }
}
