using System.Diagnostics.CodeAnalysis;
using Discord;
using GrillBot.App.Actions.Api.V1.Reminder;
using GrillBot.App.Managers;
using GrillBot.Data.Exceptions;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Reminder;

[TestClass]
public class FinishRemindTests : ApiActionTest<FinishRemind>
{
    private IUser[] Users { get; set; }

    protected override FinishRemind CreateInstance()
    {
        var message = new UserMessageBuilder(Consts.MessageId).Build();
        Users = new[]
        {
            new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetSendMessageAction(message).Build(),
            new UserBuilder(Consts.UserId + 1, Consts.Username, Consts.Discriminator).SetSendMessageAction(message).Build(),
            new UserBuilder(Consts.UserId + 5, Consts.Username, Consts.Discriminator).SetSendMessageAction(message).Build()
        };

        var client = new ClientBuilder().SetGetUserAction(Users).SetSelfUser(new SelfUserBuilder(Users[2]).Build()).Build();
        var auditLogWriter = new AuditLogWriteManager(DatabaseBuilder);
        return new FinishRemind(ApiRequestContext, DatabaseBuilder, auditLogWriter, client, TestServices.Texts.Value);
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
        => await Instance.ProcessAsync(1, false, false);

    [TestMethod]
    public async Task ProcessAsync_IsGone_AlreadyCancelled()
    {
        await InitDataAsync(Consts.UserId, Consts.UserId, 0);

        await Instance.ProcessAsync(1, false, true);
        Assert.IsTrue(Instance.IsGone);
        Assert.IsFalse(string.IsNullOrEmpty(Instance.ErrorMessage));
    }

    [TestMethod]
    public async Task ProcessAsync_IsGone_AlreadyNotified()
    {
        await InitDataAsync(Consts.UserId, Consts.UserId, Consts.MessageId);

        await Instance.ProcessAsync(1, false, true);
        Assert.IsTrue(Instance.IsGone);
        Assert.IsFalse(string.IsNullOrEmpty(Instance.ErrorMessage));
    }

    [TestMethod]
    public async Task ProcessAsync_Unauthorized_InvalidFrom()
    {
        await InitDataAsync(Consts.UserId + 1, Consts.UserId + 2, null);
        await Instance.ProcessAsync(1, false, false);

        Assert.IsFalse(Instance.IsAuthorized);
        Assert.IsFalse(string.IsNullOrEmpty(Instance.ErrorMessage));
    }

    [TestMethod]
    public async Task ProcessAsync_WithoutNotification()
    {
        await InitDataAsync(Consts.UserId, Consts.UserId, null);
        await Instance.ProcessAsync(1, false, false);

        Assert.IsFalse(Instance.IsGone);
        Assert.IsTrue(Instance.IsAuthorized);
        Assert.IsTrue(string.IsNullOrEmpty(Instance.ErrorMessage));
    }

    [TestMethod]
    public async Task ProcessAsync_WithNotification()
    {
        await InitDataAsync(Consts.UserId, Consts.UserId, null);
        await Instance.ProcessAsync(1, true, false);

        Assert.IsFalse(Instance.IsGone);
        Assert.IsTrue(Instance.IsAuthorized);
        Assert.IsTrue(string.IsNullOrEmpty(Instance.ErrorMessage));
    }

    [TestMethod]
    public void ResetState()
    {
        Instance.ResetState();
        
        Assert.IsNull(Instance.ErrorMessage);
        Assert.IsFalse(Instance.IsAuthorized);
        Assert.IsFalse(Instance.IsGone);
    }
}
