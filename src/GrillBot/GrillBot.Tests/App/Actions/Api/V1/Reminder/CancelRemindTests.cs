using System.Diagnostics.CodeAnalysis;
using Discord;
using GrillBot.App.Actions.Api.V1.Reminder;
using GrillBot.App.Services.AuditLog;
using GrillBot.Data.Exceptions;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Reminder;

[TestClass]
public class CancelRemindTests : ApiActionTest<CancelRemind>
{
    private IUser[] Users { get; set; }

    protected override CancelRemind CreateAction()
    {
        var message = new UserMessageBuilder().SetId(Consts.MessageId).Build();
        Users = new[]
        {
            new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetSendMessageAction(message).Build(),
            new UserBuilder().SetIdentity(Consts.UserId + 1, Consts.Username, Consts.Discriminator).SetSendMessageAction(message).Build()
        };

        var client = new ClientBuilder().SetGetUserAction(Users).SetSelfUser(new SelfUserBuilder(Users[0]).Build()).Build();
        var texts = new TextsBuilder()
            .AddText("RemindModule/CancelRemind/NotFound", "cs", "NotFound")
            .AddText("RemindModule/CancelRemind/AlreadyCancelled", "cs", "AlreadyCancelled")
            .AddText("RemindModule/CancelRemind/AlreadyNotified", "cs", "AlreadyNotified")
            .AddText("RemindModule/CancelRemind/InvalidOperator", "cs", "InvalidOperator")
            .AddText("RemindModule/NotifyMessage/ForceTitle", "cs", "ForceTitle")
            .AddText("RemindModule/NotifyMessage/Title", "cs", "Title")
            .AddText("RemindModule/NotifyMessage/Fields/Id", "cs", "Fields/Id")
            .AddText("RemindModule/NotifyMessage/Fields/From", "cs", "Fields/From")
            .AddText("RemindModule/NotifyMessage/Fields/Attention", "cs", "Fields/Attention")
            .AddText("RemindModule/NotifyMessage/Postponed", "cs", "Postponed")
            .AddText("RemindModule/NotifyMessage/Fields/Message", "cs", "Fields/Message")
            .AddText("RemindModule/NotifyMessage/Fields/Options", "cs", "Fields/Options")
            .AddText("RemindModule/NotifyMessage/Options", "cs", "Options")
            .Build();

        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);
        return new CancelRemind(ApiRequestContext, DatabaseBuilder, auditLogWriter, client, texts);
    }

    private async Task InitDataAsync(ulong fromUserId, ulong toUserId, ulong? remindMessageId)
    {
        foreach (var user in Users)
            await Repository.AddAsync(Database.Entity.User.FromDiscord(user));

        await Repository.AddAsync(new Database.Entity.RemindMessage
        {
            At = DateTime.MaxValue,
            Id = 1,
            Message = "RemindMessage",
            Postpone = 0,
            FromUserId = fromUserId.ToString(),
            ToUserId = toUserId.ToString(),
            OriginalMessageId = (Consts.MessageId - 1).ToString(),
            RemindMessageId = remindMessageId?.ToString(),
            Language = "cs"
        });
        await Repository.CommitAsync();
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_RemindNotFound()
        => await Action.ProcessAsync(1, false, false);

    [TestMethod]
    public async Task ProcessAsync_IsGone_AlreadyCancelled()
    {
        await InitDataAsync(Consts.UserId, Consts.UserId, 0);

        await Action.ProcessAsync(1, false, true);
        Assert.IsTrue(Action.IsGone);
        Assert.IsFalse(string.IsNullOrEmpty(Action.ErrorMessage));
    }

    [TestMethod]
    public async Task ProcessAsync_IsGone_AlreadyNotified()
    {
        await InitDataAsync(Consts.UserId, Consts.UserId, Consts.MessageId);

        await Action.ProcessAsync(1, false, true);
        Assert.IsTrue(Action.IsGone);
        Assert.IsFalse(string.IsNullOrEmpty(Action.ErrorMessage));
    }

    [TestMethod]
    public async Task ProcessAsync_Unauthorized_InvalidFrom()
    {
        await InitDataAsync(Consts.UserId + 1, Consts.UserId + 2, null);
        await Action.ProcessAsync(1, false, false);

        Assert.IsFalse(Action.IsAuthorized);
        Assert.IsFalse(string.IsNullOrEmpty(Action.ErrorMessage));
    }

    [TestMethod]
    public async Task ProcessAsync_WithoutNotification()
    {
        await InitDataAsync(Consts.UserId, Consts.UserId, null);
        await Action.ProcessAsync(1, false, false);

        Assert.IsFalse(Action.IsGone);
        Assert.IsTrue(Action.IsAuthorized);
        Assert.IsTrue(string.IsNullOrEmpty(Action.ErrorMessage));
    }

    [TestMethod]
    public async Task ProcessAsync_WithNotification()
    {
        await InitDataAsync(Consts.UserId, Consts.UserId, null);
        await Action.ProcessAsync(1, true, false);

        Assert.IsFalse(Action.IsGone);
        Assert.IsTrue(Action.IsAuthorized);
        Assert.IsTrue(string.IsNullOrEmpty(Action.ErrorMessage));
    }
}
