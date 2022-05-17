using Discord;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace GrillBot.Tests.Infrastructure.Discord;

[ExcludeFromCodeCoverage]
public class GuildBuilder : BuilderBase<IGuild>
{
    public GuildBuilder()
    {
        SetRoles(Enumerable.Empty<IRole>());
    }

    public GuildBuilder SetIdentity(ulong id, string name)
    {
        return SetId(id).SetName(name);
    }

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

    public GuildBuilder SetGetUsersAction(IEnumerable<IGuildUser> users)
    {
        Mock.Setup(o => o.GetUsersAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .Returns(Task.FromResult(users.ToList().AsReadOnly() as IReadOnlyCollection<IGuildUser>));

        return this;
    }
}
