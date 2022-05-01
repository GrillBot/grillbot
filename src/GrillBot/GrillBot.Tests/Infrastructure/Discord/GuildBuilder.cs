using Discord;

namespace GrillBot.Tests.Infrastructure.Discord;

public class GuildBuilder : BuilderBase<IGuild>
{
    public GuildBuilder SetId(ulong id)
    {
        Mock.Setup(o => o.Id).Returns(id);
        Mock.Setup(o => o.CreatedAt).Returns(SnowflakeUtils.FromSnowflake(id));
        return this;
    }

    public GuildBuilder SetName(string name)
    {
        Mock.Setup(o => o.Name).Returns(name);
        return this;
    }
}
