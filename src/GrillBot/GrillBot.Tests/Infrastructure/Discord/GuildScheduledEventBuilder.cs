using Discord;
using Moq;

namespace GrillBot.Tests.Infrastructure.Discord;

public class GuildScheduledEventBuilder : BuilderBase<IGuildScheduledEvent>
{
    public GuildScheduledEventBuilder()
    {
        Mock.Setup(o => o.ModifyAsync(It.IsAny<Action<GuildScheduledEventsProperties>>(), It.IsAny<RequestOptions>()))
            .Callback<Action<GuildScheduledEventsProperties>, RequestOptions>((func, _) => func(new GuildScheduledEventsProperties()))
            .Returns(Task.CompletedTask);
    }

    public GuildScheduledEventBuilder SetId(ulong id)
    {
        Mock.Setup(o => o.Id).Returns(id);
        return this;
    }

    public GuildScheduledEventBuilder SetCreator(IUser user)
    {
        Mock.Setup(o => o.Creator).Returns(user);
        return this;
    }
}
