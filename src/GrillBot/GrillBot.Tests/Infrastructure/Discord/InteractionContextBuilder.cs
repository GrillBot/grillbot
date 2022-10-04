using Discord;

namespace GrillBot.Tests.Infrastructure.Discord;

public class InteractionContextBuilder : BuilderBase<IInteractionContext>
{
    public InteractionContextBuilder SetGuild(IGuild guild)
    {
        Mock.Setup(o => o.Guild).Returns(guild);
        return this;
    }

    public InteractionContextBuilder SetUser(IGuildUser user)
    {
        Mock.Setup(o => o.User).Returns(user);
        return this;
    }

    public InteractionContextBuilder SetInteraction(IDiscordInteraction interaction)
    {
        Mock.Setup(o => o.Interaction).Returns(interaction);
        return this;
    }

    public InteractionContextBuilder SetChannel(IMessageChannel channel)
    {
        Mock.Setup(o => o.Channel).Returns(channel);
        return this;
    }
}
