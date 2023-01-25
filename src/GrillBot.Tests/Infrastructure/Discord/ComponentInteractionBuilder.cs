using Discord;
using Moq;

namespace GrillBot.Tests.Infrastructure.Discord;

public class ComponentInteractionBuilder : BuilderBase<IComponentInteraction>
{
    public ComponentInteractionBuilder(ulong id)
    {
        Mock.Setup(o => o.DeferAsync(It.IsAny<bool>(), It.IsAny<RequestOptions>())).Returns(Task.CompletedTask);
        Mock.Setup(o => o.UpdateAsync(It.IsAny<Action<MessageProperties>>(), It.IsAny<RequestOptions>()))
            .Callback<Action<MessageProperties>, RequestOptions>((func, _) => func(new MessageProperties()))
            .Returns(Task.CompletedTask);

        SetId(id);
    }

    public ComponentInteractionBuilder SetId(ulong id)
    {
        Mock.Setup(o => o.Id).Returns(id);
        return this;
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

    public ComponentInteractionBuilder AsDmInteraction(bool isDmInteraction = true)
    {
        Mock.Setup(o => o.IsDMInteraction).Returns(isDmInteraction);
        return this;
    }
}
