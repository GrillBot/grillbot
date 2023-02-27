using GrillBot.App.Actions.Api.V1.Auth;
using GrillBot.Data.Models.API;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.Auth;

[TestClass]
public class GetRedirectLinkTests : ApiActionTest<GetRedirectLink>
{
    protected override GetRedirectLink CreateInstance()
    {
        return new GetRedirectLink(ApiRequestContext, TestServices.Configuration.Value);
    }

    [TestMethod]
    public void Process()
    {
        var state = new AuthState { IsPublic = false, ReturnUrl = "http://localhost" };
        var link = Instance.Process(state);

        Assert.IsNotNull(link);
        Assert.IsFalse(string.IsNullOrEmpty(link.Url));
        Assert.IsTrue(link.Url.Contains("state="));
    }
}
