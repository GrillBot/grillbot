using Discord;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.Tests.TestHelpers;

[ExcludeFromCodeCoverage]
public static class DataHelper
{
    private const ulong Id = 12345;

    public static IUser CreateDiscordUser(string username = "User", ulong id = Id, string discriminator = "1111")
    {
        var mock = new Mock<IUser>();
        mock.Setup(o => o.Username).Returns(username);
        mock.Setup(o => o.Id).Returns(id);
        mock.Setup(o => o.Discriminator).Returns(discriminator);

        return mock.Object;
    }

    public static IGuildUser CreateGuildUser(string username = "User", ulong id = Id, string discriminator = "1111", string nickname = null)
    {
        var mock = new Mock<IGuildUser>();
        mock.Setup(o => o.Username).Returns(username);
        mock.Setup(o => o.Id).Returns(id);
        mock.Setup(o => o.Discriminator).Returns(discriminator);
        mock.Setup(o => o.Nickname).Returns(nickname);

        var guild = CreateGuild();
        mock.Setup(o => o.Guild).Returns(guild);
        mock.Setup(o => o.GuildId).Returns(guild.Id);

        return mock.Object;
    }

    public static IChannel CreateChannel()
    {
        var mock = new Mock<IChannel>();
        mock.Setup(o => o.Id).Returns(Id);
        mock.Setup(o => o.Name).Returns("Channel");

        return mock.Object;
    }

    public static IGuild CreateGuild()
    {
        var mock = new Mock<IGuild>();
        mock.Setup(o => o.Id).Returns(Id);
        mock.Setup(o => o.Name).Returns("Guild");
        mock.Setup(o => o.Roles).Returns(new List<IRole>() { CreateRole() });

        return mock.Object;
    }

    public static IRole CreateRole()
    {
        var mock = new Mock<IRole>();
        mock.Setup(o => o.Id).Returns(Id);

        return mock.Object;
    }

    public static IUserMessage CreateMessage(IUser author = null, string content = null)
    {
        var msg = new Mock<IUserMessage>();
        msg.Setup(o => o.Id).Returns(Id);
        msg.Setup(o => o.Author).Returns(author);
        msg.Setup(o => o.Content).Returns(content);

        return msg.Object;
    }
}
