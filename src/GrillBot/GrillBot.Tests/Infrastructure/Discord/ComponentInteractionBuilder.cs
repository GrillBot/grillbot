using System;
using Discord;
using Moq;

namespace GrillBot.Tests.Infrastructure.Discord;

public class ComponentInteractionBuilder : BuilderBase<IComponentInteraction>
{
    public ComponentInteractionBuilder()
    {
        Mock.Setup(o => o.DeferAsync(It.IsAny<bool>(), It.IsAny<RequestOptions>())).Returns(Task.CompletedTask);
        Mock.Setup(o => o.UpdateAsync(It.IsAny<Action<MessageProperties>>(), It.IsAny<RequestOptions>()))
            .Callback<Action<MessageProperties>, RequestOptions>((func, _) => func(new MessageProperties()))
            .Returns(Task.CompletedTask);
    }

    public ComponentInteractionBuilder SetGuild(IGuild guild)
    {
        Mock.Setup(o => o.GuildId).Returns(guild.Id);
        return this;
    }

    public ComponentInteractionBuilder SetMessage(IUserMessage message)
    {
        Mock.Setup(o => o.Message).Returns(message);
        return this;
    }

    public ComponentInteractionBuilder SetDmInteraction(bool isDmInteraction = true)
    {
        Mock.Setup(o => o.IsDMInteraction).Returns(isDmInteraction);
        return this;
    }
}
