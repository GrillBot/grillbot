using Discord;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.Infrastructure.Discord;

[ExcludeFromCodeCoverage]
public class ChannelBuilder : BuilderBase<IChannel>
{
    public ChannelBuilder(ulong id, string name)
    {
        SetId(id).SetName(name);
    }

    public ChannelBuilder SetId(ulong id)
    {
        Mock.Setup(o => o.Id).Returns(id);
        Mock.Setup(o => o.CreatedAt).Returns(SnowflakeUtils.FromSnowflake(id));
        return this;
    }

    public ChannelBuilder SetName(string name)
    {
        Mock.Setup(o => o.Name).Returns(name);
        return this;
    }
}
