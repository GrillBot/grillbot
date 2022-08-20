namespace GrillBot.Tests.Common;

public abstract class AttributeTest<TAttribute> where TAttribute : Attribute
{
    protected TAttribute Attribute { get; private set; }

    protected abstract TAttribute CreateAttribute();

    [TestInitialize]
    public void Initialize()
    {
        Attribute = CreateAttribute();
    }
}
