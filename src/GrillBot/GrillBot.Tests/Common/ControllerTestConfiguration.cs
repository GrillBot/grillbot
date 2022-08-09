namespace GrillBot.Tests.Common;

[AttributeUsage(AttributeTargets.Method)]
public class ControllerTestConfiguration : Attribute
{
    public bool IsPublic { get; }
    public bool CanInitProvider { get; }

    public ControllerTestConfiguration(bool isPublic = false, bool canInitProvider = false)
    {
        IsPublic = isPublic;
        CanInitProvider = canInitProvider;
    }
}
