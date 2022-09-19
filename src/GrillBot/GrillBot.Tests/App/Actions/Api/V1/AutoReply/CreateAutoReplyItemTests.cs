using GrillBot.App.Actions.Api.V1.AutoReply;
using GrillBot.App.Services;
using GrillBot.Common.Managers;
using GrillBot.Data.Models.API.AutoReply;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.AutoReply;

[TestClass]
public class CreateAutoReplyItemTests : ApiActionTest<CreateAutoReplyItem>
{
    protected override CreateAutoReplyItem CreateAction()
    {
        var discordClient = DiscordHelper.CreateClient();
        var initManager = new InitManager(TestServices.LoggerFactory.Value);
        var service = new AutoReplyService(TestServices.Configuration.Value, discordClient, DatabaseBuilder, initManager);

        return new CreateAutoReplyItem(ApiRequestContext, service, DatabaseBuilder, TestServices.AutoMapper.Value);
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
