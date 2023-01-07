using GrillBot.App.Actions.Api.V1.Invite;
using GrillBot.Cache.Services.Managers;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.Invite;

[TestClass]
public class GetMetadataCountTests : ApiActionTest<GetMetadataCount>
{
    protected override GetMetadataCount CreateAction()
    {
        var inviteManager = new InviteManager(CacheBuilder, TestServices.CounterManager.Value);
        return new GetMetadataCount(ApiRequestContext, inviteManager);
    }

    [TestMethod]
    public async Task ProcessAsync()
    {
        var result = await Action.ProcessAsync();
        Assert.AreEqual(0, result);
    }
}
