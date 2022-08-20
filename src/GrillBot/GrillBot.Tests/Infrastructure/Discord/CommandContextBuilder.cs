using Discord;
using Discord.Commands;

namespace GrillBot.Tests.Infrastructure.Discord;

public class CommandContextBuilder : BuilderBase<ICommandContext>
{
    public CommandContextBuilder SetClient(IDiscordClient client)
    {
        Mock.Setup(o => o.Client).Returns(client);
        return this;
    }

    public CommandContextBuilder SetChannel(IMessageChannel channel)
    {
        Mock.Setup(o => o.Channel).Returns(channel);
        return this;
    }

    public CommandContextBuilder SetUser(IUser user)
    {
        Mock.Setup(o => o.User).Returns(user);
        return this;
    }

    public CommandContextBuilder SetMessage(IUserMessage message)
    {
        Mock.Setup(o => o.Message).Returns(message);
        return this;
    }
}
