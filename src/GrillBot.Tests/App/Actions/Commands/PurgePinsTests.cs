using Discord;
using GrillBot.App.Actions.Commands;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands;

[TestClass]
public class PurgePinsTests : CommandActionTest<PurgePins>
{
    private static readonly IGuildUser Author = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

    private static readonly IUserMessage[] Messages =
    {
        new UserMessageBuilder(Consts.MessageId).SetAuthor(Author).AsPinned().Build(),
        new UserMessageBuilder(Consts.MessageId).SetAuthor(Author).AsPinned().Build(),
        new UserMessageBuilder(Consts.MessageId).SetAuthor(Author).AsPinned().Build(),
        new UserMessageBuilder(Consts.MessageId).SetAuthor(Author).AsPinned().Build(),
        new UserMessageBuilder(Consts.MessageId).SetAuthor(Author).AsPinned().Build(),
        new UserMessageBuilder(Consts.MessageId).SetAuthor(Author).AsPinned().Build(),
        new UserMessageBuilder(Consts.MessageId).SetAuthor(Author).AsPinned().Build(),
    };

    protected override IMessageChannel Channel { get; }
        = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGetPinnedMessagesAction(Messages).Build();

    protected override IGuildUser User => Author;

    protected override PurgePins CreateInstance()
    {
        return InitAction(new PurgePins(TestServices.Texts.Value));
    }

    [TestMethod]
    public async Task ProcessAsync_ChannelFromContext()
    {
        var result = await Instance.ProcessAsync(Messages.Length, null);

        Assert.IsFalse(string.IsNullOrEmpty(result));
        Assert.IsTrue(result.Contains(Messages.Length.ToString()));
    }

    [TestMethod]
    public async Task ProcessAsync_WithChannel()
    {
        var result = await Instance.ProcessAsync(100, (ITextChannel)Channel);

        Assert.IsFalse(string.IsNullOrEmpty(result));
        Assert.IsTrue(result.Contains(Messages.Length.ToString()));
    }
}
