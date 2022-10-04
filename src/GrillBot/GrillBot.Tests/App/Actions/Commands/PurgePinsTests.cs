using Discord;
using GrillBot.App.Actions.Commands;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands;

[TestClass]
public class PurgePinsTests : CommandActionTest<PurgePins>
{
    private static readonly IGuildUser Author = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

    private static readonly IUserMessage[] Messages =
    {
        new UserMessageBuilder().SetId(Consts.MessageId).SetAuthor(Author).AsPinned().Build(),
        new UserMessageBuilder().SetId(Consts.MessageId).SetAuthor(Author).AsPinned().Build(),
        new UserMessageBuilder().SetId(Consts.MessageId).SetAuthor(Author).AsPinned().Build(),
        new UserMessageBuilder().SetId(Consts.MessageId).SetAuthor(Author).AsPinned().Build(),
        new UserMessageBuilder().SetId(Consts.MessageId).SetAuthor(Author).AsPinned().Build(),
        new UserMessageBuilder().SetId(Consts.MessageId).SetAuthor(Author).AsPinned().Build(),
        new UserMessageBuilder().SetId(Consts.MessageId).SetAuthor(Author).AsPinned().Build(),
    };

    protected override IMessageChannel Channel { get; }
        = new TextChannelBuilder().SetIdentity(Consts.ChannelId, Consts.ChannelName).SetGetPinnedMessagesAction(Messages).Build();

    protected override IGuildUser User => Author;

    protected override PurgePins CreateAction()
    {
        var texts = new TextsBuilder().AddText("Pins/UnpinCount", "en-US", "{0}").Build();
        return InitAction(new PurgePins(texts));
    }

    [TestMethod]
    public async Task ProcessAsync_ChannelFromContext()
    {
        var result = await Action.ProcessAsync(Messages.Length, null);

        Assert.IsFalse(string.IsNullOrEmpty(result));
        Assert.AreEqual(Messages.Length.ToString(), result);
    }

    [TestMethod]
    public async Task ProcessAsync_WithChannel()
    {
        var result = await Action.ProcessAsync(100, (ITextChannel)Channel);

        Assert.IsFalse(string.IsNullOrEmpty(result));
        Assert.AreEqual(Messages.Length.ToString(), result);
    }
}
