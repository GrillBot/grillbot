using GrillBot.App.Actions.Api.V1.Reminder;
using GrillBot.Data.Models.API.Reminder;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Reminder;

[TestClass]
public class GetReminderListTests : ApiActionTest<GetReminderList>
{
    protected override GetReminderList CreateAction()
    {
        return new GetReminderList(ApiRequestContext, DatabaseBuilder, TestServices.AutoMapper.Value);
    }

    [TestMethod]
    public async Task ProcessAsync_WithoutFilter()
    {
        await InitDataAsync();

        var result = await Action.ProcessAsync(new GetReminderListParams());
        Assert.AreEqual(1, result.TotalItemsCount);
    }

    [TestMethod]
    [ControllerTestConfiguration(true)]
    public async Task ProcessAsync_AsPublic()
    {
        await InitDataAsync();

        var filter = new GetReminderListParams { Sort = { Descending = true, OrderBy = "ToUser" } };
        var result = await Action.ProcessAsync(filter);
        Assert.AreEqual(1, result.TotalItemsCount);
    }

    [TestMethod]
    public async Task ProcessAsync_AsUser_WithFilter()
    {
        var filter = new GetReminderListParams
        {
            Sort = { Descending = true, OrderBy = "ToUser" },
            CreatedFrom = DateTime.MinValue,
            CreatedTo = DateTime.MaxValue,
            MessageContains = "Test",
            OnlyWaiting = true,
            FromUserId = Consts.UserId.ToString(),
            OriginalMessageId = Consts.MessageId.ToString(),
            ToUserId = Consts.UserId.ToString()
        };

        var result = await Action.ProcessAsync(filter);
        Assert.AreEqual(0, result.TotalItemsCount);
    }

    private async Task InitDataAsync()
    {
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

        await Repository.AddAsync(Database.Entity.User.FromDiscord(user));
        await Repository.AddAsync(new Database.Entity.RemindMessage
        {
            At = DateTime.Now,
            FromUserId = Consts.UserId.ToString(),
            ToUserId = Consts.UserId.ToString(),
            Message = "Test"
        });
        await Repository.CommitAsync();
    }
}
