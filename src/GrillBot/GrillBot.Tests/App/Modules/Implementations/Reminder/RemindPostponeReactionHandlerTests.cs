using Discord;
using GrillBot.App.Modules.Implementations.Reminder;
using GrillBot.Data;
using GrillBot.Database.Entity;
using Moq;
using System;
using System.Linq;

namespace GrillBot.Tests.App.Modules.Implementations.Reminder;

[TestClass]
public class RemindPostponeReactionHandlerTests : ReactionEventHandlerTest<RemindPostponeReactionHandler>
{
    protected override RemindPostponeReactionHandler CreateHandler()
    {
        var discordClient = DiscordHelper.CreateDiscordClient();

        return new RemindPostponeReactionHandler(DbFactory, discordClient);
    }

    [TestMethod]
    public async Task OnReactionAddedAsync_NotDMs()
    {
        var message = new Mock<IUserMessage>();
        message.Setup(o => o.Channel).Returns(new Mock<IMessageChannel>().Object);

        var result = await Handler.OnReactionAddedAsync(message.Object, null, null);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task OnReactionAddedAsync_NoEmbeds()
    {
        var message = new Mock<IUserMessage>();
        message.Setup(o => o.Channel).Returns(new Mock<IDMChannel>().Object);
        message.Setup(o => o.Embeds).Returns(new List<IEmbed>().AsReadOnly());

        var result = await Handler.OnReactionAddedAsync(message.Object, null, null);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task OnReactionAddedAsync_NoEmoji()
    {
        var message = new Mock<IUserMessage>();
        message.Setup(o => o.Channel).Returns(new Mock<IDMChannel>().Object);
        message.Setup(o => o.Embeds).Returns(new List<IEmbed>() { new EmbedBuilder().Build() }.AsReadOnly());

        var emote = Emote.Parse("<:LP_FeelsHighMan:895331837822500866>");

        var result = await Handler.OnReactionAddedAsync(message.Object, emote, null);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task OnReactionAddedAsync_NotKnownEmoji()
    {
        var message = new Mock<IUserMessage>();
        message.Setup(o => o.Channel).Returns(new Mock<IDMChannel>().Object);
        message.Setup(o => o.Embeds).Returns(new List<IEmbed>() { new EmbedBuilder().Build() }.AsReadOnly());

        var result = await Handler.OnReactionAddedAsync(message.Object, Emojis.Ok, null);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task OnReactionAddedAsync_MissingBotReactions()
    {
        var message = new Mock<IUserMessage>();
        message.Setup(o => o.Channel).Returns(new Mock<IDMChannel>().Object);
        message.Setup(o => o.Embeds).Returns(new List<IEmbed>() { new EmbedBuilder().Build() }.AsReadOnly());
        message.Setup(o => o.GetReactionUsersAsync(It.IsAny<IEmote>(), It.IsAny<int>(), It.IsAny<RequestOptions>()))
            .Returns(new List<IReadOnlyCollection<IUser>>() { new List<IUser>() { DataHelper.CreateDiscordUser() } }.ToAsyncEnumerable());

        var result = await Handler.OnReactionAddedAsync(message.Object, Emojis.One, null);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task OnReactionAddedAsync_UnknownRemind()
    {
        var self = DataHelper.CreateSelfUser();
        var message = new Mock<IUserMessage>();
        message.Setup(o => o.Channel).Returns(new Mock<IDMChannel>().Object);
        message.Setup(o => o.Embeds).Returns(new List<IEmbed>() { new EmbedBuilder().Build() }.AsReadOnly());
        message.Setup(o => o.GetReactionUsersAsync(It.IsAny<IEmote>(), It.IsAny<int>(), It.IsAny<RequestOptions>()))
            .Returns(new List<IReadOnlyCollection<IUser>>() { new List<IUser>() { self } }.ToAsyncEnumerable());

        var result = await Handler.OnReactionAddedAsync(message.Object, Emojis.One, self);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task OnReactionAddedAsync_Success()
    {
        var self = DataHelper.CreateSelfUser();
        var message = new Mock<IUserMessage>();
        message.Setup(o => o.Id).Returns(425639);
        message.Setup(o => o.Channel).Returns(new Mock<IDMChannel>().Object);
        message.Setup(o => o.Embeds).Returns(new List<IEmbed>() { new EmbedBuilder().Build() }.AsReadOnly());
        message.Setup(o => o.GetReactionUsersAsync(It.IsAny<IEmote>(), It.IsAny<int>(), It.IsAny<RequestOptions>()))
            .Returns(new List<IReadOnlyCollection<IUser>>() { new List<IUser>() { self } }.ToAsyncEnumerable());
        message.Setup(o => o.DeleteAsync(It.IsAny<RequestOptions>())).Returns(Task.CompletedTask);

        var user = DataHelper.CreateDiscordUser();
        var remind = new RemindMessage()
        {
            At = DateTime.Now,
            FromUser = User.FromDiscord(user),
            FromUserId = user.Id.ToString(),
            Id = 42,
            Message = "ASDF",
            OriginalMessageId = "425639",
            Postpone = 0,
            RemindMessageId = "425639",
            ToUser = User.FromDiscord(user),
            ToUserId = user.Id.ToString()
        };

        await DbContext.AddAsync(remind);
        await DbContext.SaveChangesAsync();

        var result = await Handler.OnReactionAddedAsync(message.Object, Emojis.One, self);
        Assert.IsTrue(result);
    }
}
