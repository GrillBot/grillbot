using Discord;

namespace GrillBot.Tests.Infrastructure.Discord;

public class GuildScheduledEventBuilder : BuilderBase<IGuildScheduledEvent>
{
    public GuildScheduledEventBuilder SetId(ulong id)
    {
        Mock.Setup(o => o.Id).Returns(id);
        return this;
    }
}
