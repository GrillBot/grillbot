using Discord;
using Moq;
using System;
using System.Linq;

namespace GrillBot.Tests.Infrastructure.Discord;

public class GuildUserBuilder : BuilderBase<IGuildUser>
{
    public GuildUserBuilder()
    {
        Mock.Setup(o => o.AddRoleAsync(It.IsAny<IRole>(), It.IsAny<RequestOptions>())).Returns(Task.CompletedTask);
        Mock.Setup(o => o.AddRoleAsync(It.IsAny<ulong>(), It.IsAny<RequestOptions>())).Returns(Task.CompletedTask);
        Mock.Setup(o => o.AddRolesAsync(It.IsAny<IEnumerable<IRole>>(), It.IsAny<RequestOptions>())).Returns(Task.CompletedTask);
        Mock.Setup(o => o.AddRolesAsync(It.IsAny<IEnumerable<ulong>>(), It.IsAny<RequestOptions>())).Returns(Task.CompletedTask);

        Mock.Setup(o => o.RemoveRoleAsync(It.IsAny<IRole>(), It.IsAny<RequestOptions>())).Returns(Task.CompletedTask);
        Mock.Setup(o => o.RemoveRoleAsync(It.IsAny<ulong>(), It.IsAny<RequestOptions>())).Returns(Task.CompletedTask);
        Mock.Setup(o => o.RemoveRolesAsync(It.IsAny<IEnumerable<IRole>>(), It.IsAny<RequestOptions>())).Returns(Task.CompletedTask);
        Mock.Setup(o => o.RemoveRolesAsync(It.IsAny<IEnumerable<ulong>>(), It.IsAny<RequestOptions>())).Returns(Task.CompletedTask);
    }

    public GuildUserBuilder SetJoinDate(DateTimeOffset joinedAt)
    {
        Mock.Setup(o => o.JoinedAt).Returns(joinedAt);
        return this;
    }

    public GuildUserBuilder SetGuildPermissions(GuildPermissions permissions)
    {
        Mock.Setup(o => o.GuildPermissions).Returns(permissions);
        return this;
    }

    public GuildUserBuilder SetGuild(IGuild guild)
    {
        Mock.Setup(o => o.Guild).Returns(guild);
        return this;
    }

    public GuildUserBuilder SetNickname(string nickname)
    {
        Mock.Setup(o => o.Nickname).Returns(nickname);
        return this;
    }

    public GuildUserBuilder SetRoles(IEnumerable<ulong> roles)
    {
        Mock.Setup(o => o.RoleIds).Returns(roles.ToList().AsReadOnly());
        return this;
    }

    public GuildUserBuilder SetId(ulong id)
    {
        Mock.Setup(o => o.Id).Returns(id);
        return this;
    }

    public GuildUserBuilder SetDiscriminator(string discriminator)
    {
        var discriminatorValue = Convert.ToUInt16(discriminator);

        Mock.Setup(o => o.Discriminator).Returns(discriminator);
        Mock.Setup(o => o.DiscriminatorValue).Returns(discriminatorValue);
        Mock.Setup(o => o.GetDefaultAvatarUrl()).Returns(CDN.GetDefaultUserAvatarUrl(discriminatorValue));

        return this;
    }

    public GuildUserBuilder AsBot(bool isBot = true)
    {
        Mock.Setup(o => o.IsBot).Returns(isBot);
        return this;
    }

    public GuildUserBuilder AsWebhook(bool isWebhook = true)
    {
        Mock.Setup(o => o.IsWebhook).Returns(isWebhook);
        return this;
    }

    public GuildUserBuilder SetUsername(string username)
    {
        Mock.Setup(o => o.Username).Returns(username);
        return this;
    }

    public GuildUserBuilder SetAvatarUrlAction(string avatarUrl, ImageFormat? format = null, ushort? size = null)
    {
        Mock.Setup(o => o.GetAvatarUrl(
            format != null ? It.Is<ImageFormat>(x => x == format.Value) : It.IsAny<ImageFormat>(),
            size != null ? It.Is<ushort>(x => x == size.Value) : It.IsAny<ushort>()
        )).Returns(avatarUrl);
        return this;
    }
}
