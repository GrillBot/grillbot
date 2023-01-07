using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Discord;
using GrillBot.App.Managers;
using GrillBot.Data.Exceptions;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Managers;

[TestClass]
public class HearthbeatManagerTests : ServiceTest<HearthbeatManager>
{
    private IUser User { get; set; }

    protected override HearthbeatManager CreateService()
    {
        User = new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        return new HearthbeatManager(DatabaseBuilder);
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
    public async Task SetAsync_UserNotFound()
    {
        var context = CreateContext(false);
        await Service.SetAsync(true, context);
    }

    [TestMethod]
    public async Task SetAsync_Active_PublicAdmin() => await ProcessTestAsync(true, true);

    [TestMethod]
    public async Task SetAsync_Active_PrivateAdmin() => await ProcessTestAsync(false, true);

    [TestMethod]
    public async Task SetAsync_Inactive_PublicAdmin() => await ProcessTestAsync(true, false);

    [TestMethod]
    public async Task SetAsync_Inactive_PrivateAdmin() => await ProcessTestAsync(false, false);

    private async Task ProcessTestAsync(bool isPublic, bool isActive)
    {
        await InitDataAsync();

        var context = CreateContext(isPublic);
        await Service.SetAsync(isActive, context);
    }
}
