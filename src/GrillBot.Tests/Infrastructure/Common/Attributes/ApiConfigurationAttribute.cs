namespace GrillBot.Tests.Infrastructure.Common.Attributes;

[AttributeUsage(AttributeTargets.All)]
public class ApiConfigurationAttribute : Attribute
{
    public bool IsPublic { get; }

    public ApiConfigurationAttribute(bool isPublic = false)
    {
        IsPublic = isPublic;
    }
}
