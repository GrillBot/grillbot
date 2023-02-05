using Discord;

namespace GrillBot.Tests.Infrastructure.Discord;

public class BanBuilder : BuilderBase<IBan>
{
    public BanBuilder SetUser(IUser user)
    {
        Mock.Setup(o => o.User).Returns(user);
        return this;
    }
}
