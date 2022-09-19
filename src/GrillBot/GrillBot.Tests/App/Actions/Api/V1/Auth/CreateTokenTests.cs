using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Discord;
using GrillBot.App.Actions.Api.V1.Auth;
using GrillBot.Common.Managers.Localization;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Auth;

[TestClass]
public class CreateTokenTests : ApiActionTest<CreateToken>
{
    private IUser User { get; set; }
    private ITextsManager Texts { get; set; }
    private IDiscordClient Client { get; set; }

    protected override CreateToken CreateAction()
    {
        User = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

        Client = new ClientBuilder()
            .SetGetUserAction(User)
            .Build();
        Texts = new TextsBuilder()
            .AddText("Auth/CreateToken/UserNotFound", "cs", "UserNotFound")
            .AddText("Auth/CreateToken/PublicAdminBlocked", "cs", "PublicAdminBlocked")
            .AddText("Auth/CreateToken/PrivateAdminDisabled", "cs", "PrivateAdminBlocked")
            .Build();

        var httpClientFactory = HttpClientHelper.CreateFactory(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"id\": \"" + Consts.UserId + "\"}") });
        return new CreateToken(ApiRequestContext, httpClientFactory, Client, Texts, DatabaseBuilder, TestServices.Configuration.Value);
    }

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        await InitDatabaseAsync((int)UserFlags.WebAdmin);
        var result = await Action.ProcessAsync("SessionId", false);

        Assert.IsNotNull(result);
        Assert.IsFalse(string.IsNullOrEmpty(result.AccessToken));
        Assert.IsTrue(string.IsNullOrEmpty(result.ErrorMessage));
    }

    [TestMethod]
    public async Task ProcessAsync_Unauthorized()
    {
        var httpClientFactory = HttpClientHelper.CreateFactory(new HttpResponseMessage(HttpStatusCode.Unauthorized));
        var action = new CreateToken(ApiRequestContext, httpClientFactory, Client, Texts, DatabaseBuilder, TestServices.Configuration.Value);

        await InitDatabaseAsync(0);
        var result = await action.ProcessAsync("SessionId", false);

        Assert.IsNotNull(result);
        Assert.IsTrue(string.IsNullOrEmpty(result.AccessToken));
        Assert.IsFalse(string.IsNullOrEmpty(result.ErrorMessage));
    }

    [TestMethod]
    [ExpectedException(typeof(WebException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_InternalServerError()
    {
        var httpClientFactory = HttpClientHelper.CreateFactory(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var action = new CreateToken(ApiRequestContext, httpClientFactory, Client, Texts, DatabaseBuilder, TestServices.Configuration.Value);

        await InitDatabaseAsync(0);
        await action.ProcessAsync("SessionId", false);
    }

    [TestMethod]
    public async Task ProcessAsync_PublicAdminBlocked()
    {
        await InitDatabaseAsync((int)UserFlags.PublicAdministrationBlocked);
        var result = await Action.ProcessAsync("SessionId", true);

        Assert.IsNotNull(result);
        Assert.IsTrue(string.IsNullOrEmpty(result.AccessToken));
        Assert.IsFalse(string.IsNullOrEmpty(result.ErrorMessage));
    }

    [TestMethod]
    public async Task ProcessAsync_PrivateAdminDisabled()
    {
        await InitDatabaseAsync(0);
        var result = await Action.ProcessAsync("SessionId", false);

        Assert.IsNotNull(result);
        Assert.IsTrue(string.IsNullOrEmpty(result.AccessToken));
        Assert.IsFalse(string.IsNullOrEmpty(result.ErrorMessage));
    }

    private async Task InitDatabaseAsync(int flags)
    {
        var user = Database.Entity.User.FromDiscord(User);
        user.Flags |= flags;

        await Repository.AddAsync(user);
        await Repository.CommitAsync();
    }
}
