using System;
using Discord;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace GrillBot.Tests.Infrastructure.Discord;

[ExcludeFromCodeCoverage]
public class UserMessageBuilder : BuilderBase<IUserMessage>
{
    public UserMessageBuilder()
    {
        Mock.Setup(o => o.DeleteAsync(It.IsAny<RequestOptions>())).Returns(Task.CompletedTask);
        Mock.Setup(o => o.ModifyAsync(It.IsAny<Action<MessageProperties>>(), It.IsAny<RequestOptions>()))
            .Callback<Action<MessageProperties>, RequestOptions>((func, _) => func(new MessageProperties()))
            .Returns(Task.CompletedTask);
    }

    public UserMessageBuilder SetId(ulong id)
    {
        Mock.Setup(o => o.Id).Returns(id);
        Mock.Setup(o => o.CreatedAt).Returns(SnowflakeUtils.FromSnowflake(id));
        return this;
    }

    public UserMessageBuilder SetContent(string content)
    {
        Mock.Setup(o => o.Content).Returns(content);
        return this;
    }

    public UserMessageBuilder SetAuthor(IUser author)
    {
        Mock.Setup(o => o.Author).Returns(author);
        return this;
    }

    public UserMessageBuilder SetChannel(IMessageChannel channel)
    {
        Mock.Setup(o => o.Channel).Returns(channel);
        return this;
    }

    public UserMessageBuilder SetEmbeds(IEnumerable<IEmbed> embeds)
    {
        Mock.Setup(o => o.Embeds).Returns(embeds.ToList().AsReadOnly());
        return this;
    }

    public UserMessageBuilder SetGetReactionUsersAction(IEnumerable<IUser> users)
    {
        Mock.Setup(o => o.GetReactionUsersAsync(It.IsAny<IEmote>(), It.IsAny<int>(), It.IsAny<RequestOptions>()))
            .Returns(new List<IReadOnlyCollection<IUser>> { users.ToList().AsReadOnly() }.ToAsyncEnumerable());
        return this;
    }
}
