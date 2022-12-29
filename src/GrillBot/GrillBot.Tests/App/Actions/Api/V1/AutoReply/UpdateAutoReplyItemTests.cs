using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions.Api.V1.AutoReply;
using GrillBot.App.Managers;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API.AutoReply;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.AutoReply;

[TestClass]
public class UpdateAutoReplyItemTests : ApiActionTest<UpdateAutoReplyItem>
{
    protected override UpdateAutoReplyItem CreateAction()
    {
        var manager = new AutoReplyManager(DatabaseBuilder);
        return new UpdateAutoReplyItem(ApiRequestContext, DatabaseBuilder, TestServices.AutoMapper.Value, TestServices.Texts.Value, manager);
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
