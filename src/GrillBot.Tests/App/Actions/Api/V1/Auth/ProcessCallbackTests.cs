using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using GrillBot.App.Actions.Api.V1.Auth;
using GrillBot.Data.Models.API;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.Auth;

[TestClass]
public class ProcessCallbackTests : ApiActionTest<ProcessCallback>
{
    protected override ProcessCallback CreateAction()
    {
        var httpClientFactory = HttpClientHelper.CreateFactory(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"access_token\": \"AccessToken\"}") });
        return new ProcessCallback(ApiRequestContext, TestServices.Configuration.Value, httpClientFactory);
    }

    [TestMethod]
    public async Task ProcessAsync_WithReturnUrl()
    {
        var state = new AuthState
        {
            IsPublic = false,
            ReturnUrl = "http://localhost"
        };

        var result = await Action.ProcessAsync("code", state.Encode());

        Assert.IsFalse(string.IsNullOrEmpty(result));
        Assert.IsTrue(result.Contains("http://localhost"));
    }

    [TestMethod]
    public async Task ProcessAsync_PrivateAdmin()
    {
        var state = new AuthState { IsPublic = false };

        var result = await Action.ProcessAsync("code", state.Encode());

        Assert.IsFalse(string.IsNullOrEmpty(result));
        Assert.IsTrue(result.Contains("https://admin"));
    }

    [TestMethod]
    public async Task ProcessAsync_PublicAdmin()
    {
        var state = new AuthState { IsPublic = true };

        var result = await Action.ProcessAsync("code", state.Encode());

        Assert.IsFalse(string.IsNullOrEmpty(result));
        Assert.IsTrue(result.Contains("https://client"));
    }

    [TestMethod]
    [ExpectedException(typeof(WebException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_DiscordError()
    {
        var httpClientFactory = HttpClientHelper.CreateFactory(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var action = new ProcessCallback(ApiRequestContext, TestServices.Configuration.Value, httpClientFactory);
        await action.ProcessAsync("code", new AuthState().Encode());
    }
}
