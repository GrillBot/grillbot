using GrillBot.App.Actions.Api.V1.AutoReply;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.AutoReply;

[TestClass]
public class GetAutoReplyItemTests : ApiActionTest<GetAutoReplyItem>
{
    protected override GetAutoReplyItem CreateAction()
    {
        var texts = new TextsBuilder()
            .AddText("AutoReply/NotFound", "cs", "NotFound")
            .Build();

        return new GetAutoReplyItem(ApiRequestContext, DatabaseBuilder, TestServices.AutoMapper.Value, texts);
    }

    [TestMethod]
    public async Task ProcessAsync_Found()
    {
        await Repository.AddAsync(new Database.Entity.AutoReplyItem { Id = 1, Template = "Template", Reply = "Reply" });
        await Repository.CommitAsync();

        var (item, errMsg) = await Action.ProcessAsync(1);

        Assert.IsNotNull(item);
        Assert.IsTrue(string.IsNullOrEmpty(errMsg));
    }

    [TestMethod]
    public async Task ProcessAsync_NotFound()
    {
        var (item, errMsg) = await Action.ProcessAsync(1);

        Assert.IsNull(item);
        Assert.IsFalse(string.IsNullOrEmpty(errMsg));
    }
}
