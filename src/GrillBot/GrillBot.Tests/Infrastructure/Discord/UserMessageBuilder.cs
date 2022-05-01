using Discord;

namespace GrillBot.Tests.Infrastructure.Discord;

public class UserMessageBuilder : BuilderBase<IUserMessage>
{
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
}
