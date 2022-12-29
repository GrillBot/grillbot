using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions.Api.V1.AutoReply;
using GrillBot.Data.Exceptions;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.AutoReply;

[TestClass]
public class GetAutoReplyItemTests : ApiActionTest<GetAutoReplyItem>
{
    protected override GetAutoReplyItem CreateAction()
    {
        return new GetAutoReplyItem(ApiRequestContext, DatabaseBuilder, TestServices.AutoMapper.Value, TestServices.Texts.Value);
    }

    [TestMethod]
    public async Task ProcessAsync_Found()
    {
        await Repository.AddAsync(new Database.Entity.AutoReplyItem { Id = 1, Template = "Template", Reply = "Reply" });
        await Repository.CommitAsync();

        var item = await Action.ProcessAsync(1);

        Assert.IsNotNull(item);
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_NotFound()
    {
        await Action.ProcessAsync(1);
    }
}
