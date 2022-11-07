using Discord;
using Moq;

namespace GrillBot.Tests.Infrastructure.Discord;

public class GuildScheduledEventBuilder : BuilderBase<IGuildScheduledEvent>
{
    public GuildScheduledEventBuilder(ulong id)
    {
        Mock.Setup(o => o.ModifyAsync(It.IsAny<Action<GuildScheduledEventsProperties>>(), It.IsAny<RequestOptions>()))
            .Callback<Action<GuildScheduledEventsProperties>, RequestOptions>((func, _) => func(new GuildScheduledEventsProperties()))
            .Returns(Task.CompletedTask);
        Mock.Setup(o => o.EndAsync(It.IsAny<RequestOptions>())).Returns(Task.CompletedTask);

        SetId(id);
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

    public GuildScheduledEventBuilder SetStatus(GuildScheduledEventStatus status)
    {
        Mock.Setup(o => o.Status).Returns(status);
        return this;
    }

    public GuildScheduledEventBuilder SetEndDate(DateTimeOffset? endTime)
    {
        Mock.Setup(o => o.EndTime).Returns(endTime);
        return this;
    }
}
