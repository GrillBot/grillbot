using Discord;
using Moq;

namespace GrillBot.Tests.Infrastructure.Discord;

public class ClientBuilder
{
    private Mock<IDiscordClient> Mock { get; }

    public ClientBuilder()
    {
        Mock = new Mock<IDiscordClient>();
    }

    public ClientBuilder SetSelfUser(SelfUserBuilder builder)
    {
        Mock.Setup(o => o.CurrentUser).Returns(builder.Build());
        return this;
    }

    public IDiscordClient Build() => Mock.Object;
}
