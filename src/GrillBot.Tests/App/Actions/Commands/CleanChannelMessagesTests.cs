using System.Linq;
using Discord;
using GrillBot.App.Actions.Commands;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands;

[TestClass]
public class CleanChannelMessagesTests : CommandActionTest<CleanChannelMessages>
{
    private static readonly IGuildUser Author = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

    private static readonly IUserMessage[] Messages =
    {
        new UserMessageBuilder(SnowflakeUtils.ToSnowflake(DateTimeOffset.Now.AddDays(-30))).SetAuthor(Author).Build(),
        new UserMessageBuilder(SnowflakeUtils.ToSnowflake(DateTimeOffset.Now)).SetAuthor(Author).Build(),
        new UserMessageBuilder(SnowflakeUtils.ToSnowflake(DateTimeOffset.Now)).AsPinned().SetAuthor(Author).Build()
    };

    protected override IMessageChannel Channel { get; } = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGetMessagesAsync(Messages).Build();
    protected override IDiscordInteraction Interaction { get; } = new DiscordInteractionBuilder(Consts.InteractionId).Build();
    protected override IGuildUser User => Author;

    protected override CleanChannelMessages CreateInstance()
    {
        return InitAction(new CleanChannelMessages(TestServices.Texts.Value));
    }

    [TestMethod]
    public async Task ProcessAsync_ChannelFromContext() => await ProcessTestAsync(null);

    [TestMethod]
    public async Task ProcessAsync_WithChannel() => await ProcessTestAsync((ITextChannel)Channel);

    private async Task ProcessTestAsync(ITextChannel? channel)
    {
        var result = await Instance.ProcessAsync(int.MaxValue, channel);

        Assert.IsFalse(string.IsNullOrEmpty(result));
        Assert.AreEqual(1, result.Count(o => o == '3'));
        Assert.AreEqual(1, result.Count(o => o == '1'));
    }
}
