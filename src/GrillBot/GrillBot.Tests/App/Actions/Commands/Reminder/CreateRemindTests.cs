using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Discord;
using GrillBot.App.Actions.Commands.Reminder;
using GrillBot.Common.Helpers;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands.Reminder;

[TestClass]
public class CreateRemindTests : CommandActionTest<CreateRemind>
{
    private static readonly DateTime At = DateTime.Now.AddHours(12);
    private static readonly IGuild GuildData = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();

    protected override IGuild Guild => GuildData;

    protected override IGuildUser User
        => new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(Guild).Build();

    protected override CreateRemind CreateAction()
    {
        var texts = TestServices.Texts.Value;
        var formatHelper = new FormatHelper(texts);

        return InitAction(new CreateRemind(texts, TestServices.Configuration.Value, formatHelper, DatabaseBuilder));
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_NotInFuture()
        => await Action.ProcessAsync(null, null, DateTime.MinValue, null, 0);

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_MinimalTime()
    {
        var at = DateTime.Now.AddSeconds(10);
        await Action.ProcessAsync(null, null, at, null, 0);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_EmptyMessage()
        => await Action.ProcessAsync(null, null, At, null, 0);

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_LongMessage()
        => await Action.ProcessAsync(null, null, At, new string('-', 2048), 0);

    [TestMethod]
    public async Task ProcessAsync_SenderIsReceiver()
    {
        var result = await Action.ProcessAsync(User, User, At, Consts.MessageContent, Consts.MessageId);
        Assert.AreNotEqual(0, result);
    }

    [TestMethod]
    public async Task ProcessAsync_ReceiverIsAnotherUser()
    {
        var user = new UserBuilder(User).SetId(User.Id + 1).Build();
        var result = await Action.ProcessAsync(User, user, At, Consts.MessageContent, Consts.MessageId);
        Assert.AreNotEqual(0, result);
    }
}
