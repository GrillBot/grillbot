using Discord;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.Infrastructure.Discord;

[ExcludeFromCodeCoverage]
public class ApplicationBuilder : BuilderBase<IApplication>
{
    public ApplicationBuilder()
    {
        Mock.Setup(o => o.Name).Returns("GrillBot-Tests");
        Mock.Setup(o => o.Description).Returns("GrillBot testing");
    }

    public ApplicationBuilder SetOwner(IUser user)
    {
        Mock.Setup(o => o.Owner).Returns(user);
        return this;
    }
}
