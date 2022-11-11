using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions.Api.V1.AutoReply;
using GrillBot.App.Services;
using GrillBot.Common.Managers;
using GrillBot.Data.Exceptions;
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
        var service = new AutoReplyService(discordClient, DatabaseBuilder, initManager);
        var texts = new TextsBuilder()
            .AddText("AutoReply/NotFound", "cs", "NotFound")
            .Build();

        return new UpdateAutoReplyItem(ApiRequestContext, service, DatabaseBuilder, TestServices.AutoMapper.Value, texts);
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_NotFound()
    {
        await Action.ProcessAsync(1, new AutoReplyItemParams());
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

        Assert.IsNotNull(result);
    }
}
