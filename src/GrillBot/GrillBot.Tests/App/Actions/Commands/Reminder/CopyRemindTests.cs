using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Discord;
using GrillBot.App.Actions.Commands.Reminder;
using GrillBot.App.Services.Reminder;
using GrillBot.Common.Helpers;
using GrillBot.Data.Exceptions;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands.Reminder;

[TestClass]
public class CopyRemindTests : CommandActionTest<CopyRemind>
{
    private static readonly IGuildUser UserData = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
    private static readonly IUser AnotherUser = new UserBuilder(Consts.UserId + 1, Consts.Username, Consts.Discriminator).Build();
    private static readonly IDiscordClient DiscordClient = new ClientBuilder().SetGetUserAction(AnotherUser).SetGetUserAction(UserData).Build();

    protected override IDiscordClient Client => DiscordClient;
    protected override IGuildUser User => UserData;

    private CreateRemind CreateRemind { get; set; }

    protected override CopyRemind CreateAction()
    {
        var texts = new TextsBuilder()
            .AddText("RemindModule/Copy/RemindNotFound", "en-US", "RemindNotFound")
            .AddText("RemindModule/Copy/SelfCopy", "en-US", "SelfCopy")
            .AddText("RemindModule/Copy/WasCancelled", "en-US", "WasCancelled")
            .AddText("RemindModule/Copy/WasSent", "en-US", "WasSent")
            .AddText("RemindModule/Copy/CopyExists", "en-US", "CopyExists")
            .AddText("RemindModule/Copy/OriginalUserNotFound", "en-US", "OriginalUserNotFound")
            .Build();
        var formatHelper = new FormatHelper(texts);
        CreateRemind = new CreateRemind(texts, TestServices.Configuration.Value, formatHelper, DatabaseBuilder);

        return InitAction(new CopyRemind(DatabaseBuilder, texts, CreateRemind));
    }

    private async Task InitDataAsync(string remindMessageId)
    {
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(AnotherUser));
        await Repository.AddAsync(new Database.Entity.RemindMessage
        {
            At = DateTime.Now.AddHours(12),
            Message = "Message",
            Language = "en-US",
            FromUserId = Consts.UserId.ToString(),
            OriginalMessageId = Consts.MessageId.ToString(),
            ToUserId = AnotherUser.Id.ToString(),
            RemindMessageId = remindMessageId,
            Id = 1
        });

        await Repository.CommitAsync();
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_NotFound()
        => await Action.ProcessAsync(0);

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_SameUsers()
    {
        CreateRemind.Init(Context);
        var remindId = await CreateRemind.ProcessAsync(Context.User, Context.User, DateTime.Now.AddHours(12), "Message", Consts.MessageId);
        await Action.ProcessAsync(remindId);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_Cancelled()
    {
        await InitDataAsync(RemindHelper.NotSentRemind);
        await Action.ProcessAsync(1);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_WasSent()
    {
        await InitDataAsync(Consts.MessageId.ToString());
        await Action.ProcessAsync(1);
    }

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        await InitDataAsync(null);
        await Action.ProcessAsync(1);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_CopyExists()
    {
        await InitDataAsync(null);
        await Action.ProcessAsync(1);
        await Action.ProcessAsync(1);
    }
}
