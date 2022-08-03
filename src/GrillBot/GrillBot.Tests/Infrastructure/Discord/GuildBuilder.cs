using Discord;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Org.BouncyCastle.Asn1.Ocsp;

namespace GrillBot.Tests.Infrastructure.Discord;

[ExcludeFromCodeCoverage]
public class GuildBuilder : BuilderBase<IGuild>
{
    private IRole EveryoneRole { get; }

    public GuildBuilder()
    {
        EveryoneRole = new RoleBuilder().SetIdentity(Consts.GuildId, "@everyone").Build();

        SetRoles(new[] { EveryoneRole });
        SetEveryoneRole(EveryoneRole);
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
        var rolesData = roles.ToList();
        if (!rolesData.Exists(o => o.Id == EveryoneRole.Id))
            rolesData.Add(EveryoneRole);

        Mock.Setup(o => o.Roles).Returns(rolesData.AsReadOnly());
        foreach (var role in rolesData.Where(o => o.Id != EveryoneRole.Id))
            SetGetRoleAction(role);
        return this;
    }

    public GuildBuilder SetEveryoneRole(IRole role)
    {
        Mock.Setup(o => o.EveryoneRole).Returns(role);
        SetGetRoleAction(role);
        return this;
    }

    public GuildBuilder SetGetRoleAction(IRole role)
    {
        Mock.Setup(o => o.GetRole(It.Is<ulong>(id => id == role.Id))).Returns(role);
        return this;
    }

    public GuildBuilder SetGetTextChannelAction(ITextChannel channel)
    {
        Mock.Setup(o =>
            o.GetTextChannelAsync(It.Is<ulong>(x => x == channel.Id), It.IsAny<CacheMode>(),
                It.IsAny<RequestOptions>())).Returns(Task.FromResult(channel));
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
        var channelsData = channels.ToList().AsReadOnly();

        Mock.Setup(o => o.GetChannelsAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .Returns(Task.FromResult((IReadOnlyCollection<IGuildChannel>)channelsData));

        SetGetTextChannelsAction(channelsData);
        return this;
    }

    public GuildBuilder SetGetUsersAction(IEnumerable<IGuildUser> users)
    {
        var usersData = users.ToList().AsReadOnly();

        Mock.Setup(o => o.GetUsersAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .Returns(Task.FromResult((IReadOnlyCollection<IGuildUser>)usersData));
        foreach (var user in usersData)
            SetGetUserAction(user);

        return this;
    }

    public GuildBuilder SetGetUserAction(IGuildUser user)
    {
        Mock.Setup(o =>
                o.GetUserAsync(It.Is<ulong>(id => id == user.Id), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .Returns(Task.FromResult(user));
        return this;
    }
}