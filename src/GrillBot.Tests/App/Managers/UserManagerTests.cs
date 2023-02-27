using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Discord;
using GrillBot.App.Managers;
using GrillBot.Data.Exceptions;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Managers;

[TestClass]
public class UserManagerTests : TestBase<UserManager>
{
    private IUser User { get; set; } = null!;

    protected override UserManager CreateInstance()
    {
        return new UserManager(DatabaseBuilder);
    }

    protected override void PreInit()
    {
        User = new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.CommitAsync();
    }

    private ApiRequestContext CreateContext(bool isPublic)
    {
        return new ApiRequestContext
        {
            LoggedUserData = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, !isPublic ? "Admin" : "User"),
                new Claim(ClaimTypes.NameIdentifier, Consts.UserId.ToString())
            })),
            LoggedUser = User
        };
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task SetHearthbeatAsync_UserNotFound()
    {
        var context = CreateContext(false);
        await Instance.SetHearthbeatAsync(true, context);
    }

    [TestMethod]
    public async Task SetHearthbeatAsync_Active_PublicAdmin() => await ProcessTestAsync(true, true);

    [TestMethod]
    public async Task SetHearthbeatAsync_Active_PrivateAdmin() => await ProcessTestAsync(false, true);

    [TestMethod]
    public async Task SetHearthbeatAsync_Inactive_PublicAdmin() => await ProcessTestAsync(true, false);

    [TestMethod]
    public async Task SetHearthbeatAsync_Inactive_PrivateAdmin() => await ProcessTestAsync(false, false);

    private async Task ProcessTestAsync(bool isPublic, bool isActive)
    {
        await InitDataAsync();

        var context = CreateContext(isPublic);
        await Instance.SetHearthbeatAsync(isActive, context);
    }

    [TestMethod]
    public async Task CheckFlagsAsync_NotFound()
    {
        var result = await Instance.CheckFlagsAsync(User, UserFlags.BotAdmin);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task CheckFlags_Ok()
    {
        await InitDataAsync();

        var result = await Instance.CheckFlagsAsync(User, UserFlags.BotAdmin);
        Assert.IsFalse(result);
    }
}
