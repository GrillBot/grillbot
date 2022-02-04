using Microsoft.Extensions.DependencyInjection;
using System;

namespace GrillBot.Tests.TestHelpers;

public static class DIHelper
{
    public static IServiceProvider CreateEmptyProvider()
    {
        return new ServiceCollection().BuildServiceProvider();
    }
}
