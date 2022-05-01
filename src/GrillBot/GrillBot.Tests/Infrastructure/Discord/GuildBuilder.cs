using Discord;
using Moq;
using System.Linq;

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

    public GuildBuilder SetRoles(IEnumerable<IRole> roles)
    {
        Mock.Setup(o => o.Roles).Returns(roles.ToList().AsReadOnly());
        return this;
    }

    public GuildBuilder SetGetTextChannelAction(ITextChannel channel)
    {
        Mock.Setup(o => o.GetTextChannelAsync(It.Is<ulong>(x => x == channel.Id), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(channel));
        return this;
    }

    public GuildBuilder SetGetTextChannelsAction(IEnumerable<ITextChannel> channels)
    {
        Mock.Setup(o => o.GetTextChannelsAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .Returns(Task.FromResult(channels.ToList().AsReadOnly() as IReadOnlyCollection<ITextChannel>));
        return this;
    }
}
