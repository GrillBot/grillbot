using System.Reflection;

namespace GrillBot.Tests.Common;

public abstract class TestBase
{
    public TestContext TestContext { get; set; }

    protected MethodInfo GetMethod()
    {
        var type = GetType();
        return type.GetMethod(TestContext.ManagedMethod, BindingFlags.Instance | BindingFlags.Public);
    }
}
