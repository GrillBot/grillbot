using Microsoft.AspNetCore.Hosting;

namespace GrillBot.Tests.Infrastructure;

public class EnvironmentBuilder : BuilderBase<IWebHostEnvironment>
{
    public EnvironmentBuilder()
    {
        Mock.Setup(o => o.ApplicationName).Returns("GrillBot-Test");
    }

    public EnvironmentBuilder AsDev() => SetName("Development");
    public EnvironmentBuilder AsProd() => SetName("Production");
    public EnvironmentBuilder AsTest() => SetName("Testing");

    private EnvironmentBuilder SetName(string name)
    {
        Mock.Setup(o => o.EnvironmentName).Returns(name);
        return this;
    }
}
