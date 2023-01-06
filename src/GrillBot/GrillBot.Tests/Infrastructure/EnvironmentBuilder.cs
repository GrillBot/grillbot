using Microsoft.AspNetCore.Hosting;

namespace GrillBot.Tests.Infrastructure;

public class EnvironmentBuilder : BuilderBase<IWebHostEnvironment>
{
    public EnvironmentBuilder AsTest() => SetName("Testing");

    private EnvironmentBuilder SetName(string name)
    {
        Mock.Setup(o => o.EnvironmentName).Returns(name);
        return this;
    }
}
