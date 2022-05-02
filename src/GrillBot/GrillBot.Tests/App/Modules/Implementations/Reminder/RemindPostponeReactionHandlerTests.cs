using Discord;
using GrillBot.App.Modules.Implementations.Reminder;
using GrillBot.Data;
using GrillBot.Database.Entity;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;
using Moq;
using System;

namespace GrillBot.Tests.App.Modules.Implementations.Reminder;

[TestClass]
public class RemindPostponeReactionHandlerTests : ReactionEventHandlerTest<RemindPostponeReactionHandler>
{
    protected override RemindPostponeReactionHandler CreateHandler()
    {
        var selfUser = new SelfUserBuilder()
            .SetId(Consts.UserId).SetUsername(Consts.Username)
            .SetDiscriminator(Consts.Discriminator).AsBot()
            .Build();

        var discordClient = new ClientBuilder()
            .SetSelfUser(selfUser).Build();

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
        var guild = new GuildBuilder()
            .SetId(Consts.GuildId).SetName(Consts.GuildName)
            .Build();

        var channel = new TextChannelBuilder()
            .SetName(Consts.ChannelName).SetId(Consts.ChannelId)
            .SetGuild(guild).Build();

        var guildUser = new GuildUserBuilder()
            .SetId(Consts.UserId).SetUsername(Consts.Username)
            .SetDiscriminator(Consts.Discriminator).SetGuild(guild)
            .Build();

        var message = new UserMessageBuilder()
            .SetChannel(channel).SetEmbeds(new[] { new EmbedBuilder().Build() })
            .SetGetReactionUsersAction(new[] { guildUser }).Build();

        var result = await Handler.OnReactionAddedAsync(message, Emojis.One, null);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task OnReactionAddedAsync_UnknownRemind()
    {
        var selfUser = new SelfUserBuilder()
            .SetId(Consts.UserId).SetUsername(Consts.Username).SetDiscriminator(Consts.Discriminator)
            .AsBot().Build();

        var dmChannel = new DmChannelBuilder().Build();

        var message = new UserMessageBuilder()
            .SetChannel(dmChannel).SetEmbeds(new[] { new EmbedBuilder().Build() })
            .SetGetReactionUsersAction(new[] { selfUser }).Build();

        var result = await Handler.OnReactionAddedAsync(message, Emojis.One, selfUser);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task OnReactionAddedAsync_Success()
    {
        var selfUser = new SelfUserBuilder()
            .SetId(Consts.UserId).SetUsername(Consts.Username).SetDiscriminator(Consts.Discriminator)
            .AsBot().Build();

        var message = new UserMessageBuilder()
            .SetId(Consts.MessageId).SetChannel(new DmChannelBuilder().Build())
            .SetEmbeds(new[] { new EmbedBuilder().Build() }).SetGetReactionUsersAction(new[] { selfUser })
            .Build();

        var user = new UserBuilder()
            .SetId(Consts.UserId).SetUsername(Consts.Username).SetDiscriminator(Consts.Discriminator)
            .Build();

        var remind = new RemindMessage()
        {
            At = DateTime.Now,
            FromUser = User.FromDiscord(user),
            FromUserId = user.Id.ToString(),
            Id = 42,
            Message = "ASDF",
            OriginalMessageId = "425639",
            Postpone = 0,
            RemindMessageId = Consts.MessageId.ToString(),
            ToUser = User.FromDiscord(user),
            ToUserId = user.Id.ToString()
        };

        await DbContext.AddAsync(remind);
        await DbContext.SaveChangesAsync();

        var result = await Handler.OnReactionAddedAsync(message, Emojis.One, selfUser);
        Assert.IsTrue(result);
    }
}
