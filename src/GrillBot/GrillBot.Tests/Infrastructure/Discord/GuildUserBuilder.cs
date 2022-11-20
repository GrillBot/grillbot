using Discord;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace GrillBot.Tests.Infrastructure.Discord;

[ExcludeFromCodeCoverage]
public class GuildUserBuilder : BuilderBase<IGuildUser>
{
    public GuildUserBuilder(ulong id, string username, string discriminator)
    {
        Mock.Setup(o => o.AddRoleAsync(It.IsAny<IRole>(), It.IsAny<RequestOptions>())).Returns(Task.CompletedTask);
        Mock.Setup(o => o.AddRoleAsync(It.IsAny<ulong>(), It.IsAny<RequestOptions>())).Returns(Task.CompletedTask);
        Mock.Setup(o => o.AddRolesAsync(It.IsAny<IEnumerable<IRole>>(), It.IsAny<RequestOptions>()))
            .Returns(Task.CompletedTask);
        Mock.Setup(o => o.AddRolesAsync(It.IsAny<IEnumerable<ulong>>(), It.IsAny<RequestOptions>()))
            .Returns(Task.CompletedTask);

        Mock.Setup(o => o.RemoveRoleAsync(It.IsAny<IRole>(), It.IsAny<RequestOptions>())).Returns(Task.CompletedTask);
        Mock.Setup(o => o.RemoveRoleAsync(It.IsAny<ulong>(), It.IsAny<RequestOptions>())).Returns(Task.CompletedTask);
        Mock.Setup(o => o.RemoveRolesAsync(It.IsAny<IEnumerable<IRole>>(), It.IsAny<RequestOptions>()))
            .Returns(Task.CompletedTask);
        Mock.Setup(o => o.RemoveRolesAsync(It.IsAny<IEnumerable<ulong>>(), It.IsAny<RequestOptions>()))
            .Returns(Task.CompletedTask);

        SetId(id);
        SetUsername(username);
        SetDiscriminator(discriminator);
    }

    public GuildUserBuilder(IUser user) : this(user.Id, user.Username, user.Discriminator)
    {
    }

    public GuildUserBuilder SetGuildPermissions(GuildPermissions permissions)
    {
        Mock.Setup(o => o.GuildPermissions).Returns(permissions);
        return this;
    }

    public GuildUserBuilder SetGuild(IGuild guild)
    {
        Mock.Setup(o => o.Guild).Returns(guild);
        Mock.Setup(o => o.GuildId).Returns(guild.Id);
        return this;
    }

    public GuildUserBuilder SetRoles(IEnumerable<ulong> roles)
    {
        Mock.Setup(o => o.RoleIds).Returns(roles.ToList().AsReadOnly());
        return this;
    }

    public GuildUserBuilder SetRoles(IEnumerable<IRole> roles)
        => SetRoles(roles.Select(o => o.Id));

    public GuildUserBuilder SetId(ulong id)
    {
        Mock.Setup(o => o.Id).Returns(id);
        Mock.Setup(o => o.CreatedAt).Returns(SnowflakeUtils.FromSnowflake(id));
        Mock.Setup(o => o.JoinedAt).Returns(SnowflakeUtils.FromSnowflake(id * 2));
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

        if (isBot)
            Mock.Setup(o => o.IsWebhook).Returns(false);

        return this;
    }

    public GuildUserBuilder SetUsername(string username)
    {
        Mock.Setup(o => o.Username).Returns(username);
        return this;
    }

    public GuildUserBuilder SetAvatar(string avatarId)
    {
        Mock.Setup(o => o.AvatarId).Returns(avatarId);
        Mock.Setup(o => o.DisplayAvatarId).Returns(avatarId);
        return this;
    }

    public GuildUserBuilder SetActiveDevices(IEnumerable<ClientType> clients)
    {
        Mock.Setup(o => o.ActiveClients).Returns(clients.ToList().AsReadOnly());
        return this;
    }

    public GuildUserBuilder SetStatus(UserStatus status, bool setActiveClients = true)
    {
        Mock.Setup(o => o.Status).Returns(status);
        if (setActiveClients && status != UserStatus.Offline)
            SetActiveDevices(new[] { ClientType.Desktop, ClientType.Mobile, ClientType.Web });
        return this;
    }

    public GuildUserBuilder SetSendMessageAction(IUserMessage message, bool disabledDms = false)
    {
        var dmChannel = new DmChannelBuilder().SetSendMessageAction(message, disabledDms).Build();
        Mock.Setup(o => o.CreateDMChannelAsync(It.IsAny<RequestOptions>())).ReturnsAsync(dmChannel);
        return this;
    }

    public GuildUserBuilder SetPremiumSinceDate(DateTimeOffset? premiumSince)
    {
        Mock.Setup(o => o.PremiumSince).Returns(premiumSince);
        return this;
    }
}
