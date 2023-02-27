using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Discord;
using GrillBot.App.Actions.Commands.Reminder;
using GrillBot.App.Helpers;
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

    protected override CopyRemind CreateInstance()
    {
        var texts = TestServices.Texts.Value;
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
        => await Instance.ProcessAsync(0);

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_SameUsers()
    {
        CreateRemind.Init(Context);
        var remindId = await CreateRemind.ProcessAsync(Context.User, Context.User, DateTime.Now.AddHours(12), "Message", Consts.MessageId);
        await Instance.ProcessAsync(remindId);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_Cancelled()
    {
        await InitDataAsync(RemindHelper.NotSentRemind);
        await Instance.ProcessAsync(1);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_WasSent()
    {
        await InitDataAsync(Consts.MessageId.ToString());
        await Instance.ProcessAsync(1);
    }

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        await InitDataAsync(null);
        await Instance.ProcessAsync(1);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_CopyExists()
    {
        await InitDataAsync(null);
        await Instance.ProcessAsync(1);
        await Instance.ProcessAsync(1);
    }
}
