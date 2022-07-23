using Discord;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Org.BouncyCastle.Asn1.Ocsp;

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

    public GuildBuilder SetGetRoleAction(IRole role)
    {
        Mock.Setup(o => o.GetRole(It.Is<ulong>(id => id == role.Id))).Returns(role);
        return this;
    }

    public GuildBuilder SetGetTextChannelAction(ITextChannel channel)
    {
        Mock.Setup(o => o.GetTextChannelAsync(It.Is<ulong>(x => x == channel.Id), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(channel));
        return this;
    }

    public GuildBuilder SetGetTextChannelsAction(IEnumerable<ITextChannel> channels)
    {
        var channelsData = channels.ToList();
        
        Mock.Setup(o => o.GetTextChannelsAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .Returns(Task.FromResult(channelsData.AsReadOnly() as IReadOnlyCollection<ITextChannel>));
        foreach (var channel in channelsData)
            SetGetTextChannelAction(channel);
        return this;
    }
    
    public GuildBuilder SetGetChannelsAction(IEnumerable<ITextChannel> channels)
    {
        Mock.Setup(o => o.GetChannelsAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .Returns(Task.FromResult(channels.ToList().AsReadOnly() as IReadOnlyCollection<IGuildChannel>));
        return this;
    }

    public GuildBuilder SetGetUsersAction(IEnumerable<IGuildUser> users)
    {
        Mock.Setup(o => o.GetUsersAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .Returns(Task.FromResult(users.ToList().AsReadOnly() as IReadOnlyCollection<IGuildUser>));

        return this;
    }

    public GuildBuilder SetGetUserAction(IGuildUser user)
    {
        Mock.Setup(o => o.GetUserAsync(It.Is<ulong>(id => id == user.Id), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(user));
        return this;
    }
}
