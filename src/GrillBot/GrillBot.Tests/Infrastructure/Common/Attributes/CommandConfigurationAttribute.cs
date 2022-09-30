namespace GrillBot.Tests.Infrastructure.Common.Attributes;

public class CommandConfigurationAttribute : TestConfigurationAttribute
{
    public CommandConfigurationAttribute(bool canInitProvider) : base(canInitProvider)
    {
    }
}
