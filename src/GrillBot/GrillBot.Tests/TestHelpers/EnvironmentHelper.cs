using Microsoft.AspNetCore.Hosting;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.TestHelpers;

[ExcludeFromCodeCoverage]
public static class EnvironmentHelper
{
    public static IWebHostEnvironment CreateEnv(string envName)
    {
        var mock = new Mock<IWebHostEnvironment>();
        mock.Setup(o => o.EnvironmentName).Returns(envName);

        return mock.Object;
    }
}
