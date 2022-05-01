using Discord;

namespace GrillBot.Tests.Infrastructure.Discord;

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
