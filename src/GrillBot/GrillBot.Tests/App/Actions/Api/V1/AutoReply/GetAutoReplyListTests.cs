using GrillBot.App.Actions.Api.V1.AutoReply;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.AutoReply;

[TestClass]
public class GetAutoReplyListTests : ApiActionTest<GetAutoReplyList>
{
    protected override GetAutoReplyList CreateAction()
    {
        return new GetAutoReplyList(ApiRequestContext, DatabaseBuilder, TestServices.AutoMapper.Value);
    }

    [TestMethod]
    public async Task ProcessAsync()
    {
        var result = await Action.ProcessAsync();

        Assert.AreEqual(0, result.Count);
    }
}
