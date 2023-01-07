namespace GrillBot.Tests.Infrastructure.Common.Attributes;

public class ApiConfigurationAttribute : TestConfigurationAttribute
{
    public bool IsPublic { get; }

    public ApiConfigurationAttribute(bool isPublic = false, bool canInitProvider = false) : base(canInitProvider)
    {
        IsPublic = isPublic;
    }
}
