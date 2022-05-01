using Discord;

namespace GrillBot.Tests.Infrastructure.Discord;

public class ThreadBuilder : BuilderBase<IThreadChannel>
{
    public ThreadBuilder SetId(ulong id)
    {
        Mock.Setup(o => o.Id).Returns(id);
        Mock.Setup(o => o.CreatedAt).Returns(SnowflakeUtils.FromSnowflake(id));
        return this;
    }

    public ThreadBuilder SetName(string name)
    {
        Mock.Setup(o => o.Name).Returns(name);
        return this;
    }

    public ThreadBuilder SetGuild(IGuild guild)
    {
        Mock.Setup(o => o.Guild).Returns(guild);
        Mock.Setup(o => o.GuildId).Returns(guild.Id);
        return this;
    }

    public ThreadBuilder SetType(ThreadType type)
    {
        Mock.Setup(o => o.Type).Returns(type);
        return this;
    }

    public ThreadBuilder IsArchived(bool isArchived = false)
    {
        Mock.Setup(o => o.IsArchived).Returns(isArchived);
        return this;
    }
}
