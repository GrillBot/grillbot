using Discord;
using Moq;
using System;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.TestHelpers;

[ExcludeFromCodeCoverage]
public static class DataHelper
{
    private const ulong Id = 12345;

    public static IUser CreateDiscordUser(string username = "User", ulong id = Id, string discriminator = "1111", UserStatus userStatus = UserStatus.Online)
    {
        var mock = new Mock<IUser>();
        mock.Setup(o => o.Username).Returns(username);
        mock.Setup(o => o.Id).Returns(id);
        mock.Setup(o => o.Discriminator).Returns(discriminator);
        mock.Setup(o => o.Status).Returns(userStatus);

        return mock.Object;
    }

    public static IGuildUser CreateGuildUser(string username = "User", ulong id = Id, string discriminator = "1111", string nickname = null, bool bot = false)
    {
        var mock = new Mock<IGuildUser>();
        mock.Setup(o => o.Username).Returns(username);
        mock.Setup(o => o.Id).Returns(id);
        mock.Setup(o => o.Discriminator).Returns(discriminator);
        mock.Setup(o => o.Nickname).Returns(nickname);
        mock.Setup(o => o.IsBot).Returns(bot);

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

    public static ITextChannel CreateTextChannel(Action<Mock<ITextChannel>> setup = null)
    {
        var mock = new Mock<ITextChannel>();
        mock.Setup(o => o.Id).Returns(Id);
        mock.Setup(o => o.Name).Returns("TextChannel");

        mock.Setup(o => o.SendFileAsync(
            It.IsAny<FileAttachment>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Embed>(),
            It.IsAny<RequestOptions>(), It.IsAny<AllowedMentions>(), It.IsAny<MessageReference>(),
            It.IsAny<MessageComponent>(), It.IsAny<ISticker[]>(), It.IsAny<Embed[]>(), It.IsAny<MessageFlags>()
        )).Returns(Task.FromResult<IUserMessage>(null));

        setup?.Invoke(mock);
        return mock.Object;
    }

    public static IGuild CreateGuild(Action<Mock<IGuild>> setup = null)
    {
        var mock = new Mock<IGuild>();
        mock.Setup(o => o.Id).Returns(Id);
        mock.Setup(o => o.Name).Returns("Guild");
        mock.Setup(o => o.Roles).Returns(new List<IRole>() { CreateRole() });

        var channel = CreateTextChannel();
        mock.Setup(o => o.GetTextChannelAsync(It.Is<ulong>(x => x == channel.Id), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(channel));
        mock.Setup(o => o.GetTextChannelsAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(new List<ITextChannel>() { channel }.AsReadOnly() as IReadOnlyCollection<ITextChannel>));

        setup?.Invoke(mock);
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

    public static ISelfUser CreateSelfUser()
    {
        var mock = new Mock<ISelfUser>();

        mock.Setup(o => o.Id).Returns(1234567890);
        mock.Setup(o => o.IsBot).Returns(true);
        mock.Setup(o => o.Username).Returns("Bot");
        mock.Setup(o => o.Discriminator).Returns("1111");

        return mock.Object;
    }

    public static IEmote CreateEmote()
        => Emote.Parse("<:Online:856875667379585034>");

    public static IAttachment CreateAttachment()
    {
        var mock = new Mock<IAttachment>();

        mock.Setup(o => o.Filename).Returns("File.png");
        mock.Setup(o => o.Url).Returns("https://www.google.com/images/searchbox/desktop_searchbox_sprites318_hr.png");
        mock.Setup(o => o.ProxyUrl).Returns("https://www.google.com/images/searchbox/desktop_searchbox_sprites318_hr.png");

        return mock.Object;
    }

    public static IApplication CreateApplication()
    {
        var mock = new Mock<IApplication>();

        mock.Setup(o => o.Owner).Returns(CreateSelfUser());

        return mock.Object;
    }

    public static IThreadChannel CreateThread(Action<Mock<IThreadChannel>> setup = null)
    {
        var mock = new Mock<IThreadChannel>();
        mock.Setup(o => o.Id).Returns(Id + 1);
        mock.Setup(o => o.Name).Returns("TextChannel");
        mock.Setup(o => o.CategoryId).Returns(Id);

        mock.Setup(o => o.SendFileAsync(
            It.IsAny<FileAttachment>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Embed>(),
            It.IsAny<RequestOptions>(), It.IsAny<AllowedMentions>(), It.IsAny<MessageReference>(),
            It.IsAny<MessageComponent>(), It.IsAny<ISticker[]>(), It.IsAny<Embed[]>(), It.IsAny<MessageFlags>()
        )).Returns(Task.FromResult<IUserMessage>(null));

        setup?.Invoke(mock);
        return mock.Object;
    }
}
