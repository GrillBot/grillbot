namespace GrillBot.Tests.Infrastructure.Common.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class TestConfigurationAttribute : Attribute
{
    public bool CanInitProvider { get; }

    public TestConfigurationAttribute(bool canInitProvider)
    {
        CanInitProvider = canInitProvider;
    }
}
