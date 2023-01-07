using GrillBot.App.Actions.Api.V1.AutoReply;
using GrillBot.App.Managers;
using GrillBot.Data.Models.API.AutoReply;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.AutoReply;

[TestClass]
public class CreateAutoReplyItemTests : ApiActionTest<CreateAutoReplyItem>
{
    protected override CreateAutoReplyItem CreateAction()
    {
        var manager = new AutoReplyManager(DatabaseBuilder);
        return new CreateAutoReplyItem(ApiRequestContext, manager, DatabaseBuilder, TestServices.AutoMapper.Value);
    }

    [TestMethod]
    public async Task ProcessAsync()
    {
        var parameters = new AutoReplyItemParams
        {
            Flags = 0,
            Reply = "Reply",
            Template = "Template"
        };

        var result = await Action.ProcessAsync(parameters);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Id > 0);
    }
}
