using GrillBot.App.Actions.Api.V1.AutoReply;
using GrillBot.App.Services;
using GrillBot.Common.Managers;
using GrillBot.Data.Models.API.AutoReply;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.AutoReply;

[TestClass]
public class UpdateAutoReplyItemTests : ApiActionTest<UpdateAutoReplyItem>
{
    protected override UpdateAutoReplyItem CreateAction()
    {
        var discordClient = DiscordHelper.CreateClient();
        var initManager = new InitManager(TestServices.LoggerFactory.Value);
        var service = new AutoReplyService(TestServices.Configuration.Value, discordClient, DatabaseBuilder, initManager);
        var texts = new TextsBuilder()
            .AddText("AutoReply/NotFound", "cs", "NotFound")
            .Build();

        return new UpdateAutoReplyItem(ApiRequestContext, service, DatabaseBuilder, TestServices.AutoMapper.Value, texts);
    }

    [TestMethod]
    public async Task ProcessAsync_NotFound()
    {
        var result = await Action.ProcessAsync(1, new AutoReplyItemParams());

        Assert.IsNull(result.item);
        Assert.IsFalse(string.IsNullOrEmpty(result.errMsg));
    }

    [TestMethod]
    public async Task ProcessAsync_Found()
    {
        await Repository.AddAsync(new Database.Entity.AutoReplyItem { Id = 1, Flags = 2, Reply = "Reply", Template = "Template" });
        await Repository.CommitAsync();

        var result = await Action.ProcessAsync(1, new AutoReplyItemParams
        {
            Flags = 2,
            Reply = "Reply",
            Template = "Template"
        });

        Assert.IsNotNull(result.item);
        Assert.IsTrue(string.IsNullOrEmpty(result.errMsg));
    }
}
