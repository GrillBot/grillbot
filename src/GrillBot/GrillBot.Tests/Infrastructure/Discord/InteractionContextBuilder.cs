using Discord;

namespace GrillBot.Tests.Infrastructure.Discord;

public class InteractionContextBuilder : BuilderBase<IInteractionContext>
{
    public InteractionContextBuilder SetGuild(IGuild guild)
    {
        Mock.Setup(o => o.Guild).Returns(guild);
        return this;
    }
}
