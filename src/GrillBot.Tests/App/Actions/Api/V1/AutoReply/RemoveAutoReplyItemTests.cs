using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions.Api.V1.AutoReply;
using GrillBot.App.Managers;
using GrillBot.Data.Exceptions;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.AutoReply;

[TestClass]
public class RemoveAutoReplyItemTests : ApiActionTest<RemoveAutoReplyItem>
{
    protected override RemoveAutoReplyItem CreateInstance()
    {
        var manager = new AutoReplyManager(DatabaseBuilder);
        return new RemoveAutoReplyItem(ApiRequestContext, DatabaseBuilder, TestServices.Texts.Value, manager);
    }

    [TestMethod]
    public async Task ProcessAsync_Found()
    {
        await Repository.AddAsync(new Database.Entity.AutoReplyItem { Id = 1, Flags = 2, Reply = "Reply", Template = "Template" });
        await Repository.CommitAsync();

        await Instance.ProcessAsync(1);
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_NotFound()
    {
        await Instance.ProcessAsync(1);
    }
}
