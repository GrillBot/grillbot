using System.Diagnostics.CodeAnalysis;
using Moq;

namespace GrillBot.Tests.Infrastructure;

[ExcludeFromCodeCoverage]
public abstract class BuilderBase<T> where T : class
{
    protected Mock<T> Mock { get; }

    protected BuilderBase()
    {
        Mock = new Mock<T>();
    }

    public virtual T Build() => Mock.Object;
}
