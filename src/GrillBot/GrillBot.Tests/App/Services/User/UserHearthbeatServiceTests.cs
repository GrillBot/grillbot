using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using GrillBot.App.Services.User;
using GrillBot.Data.Exceptions;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Services.User;

[TestClass]
public class UserHearthbeatServiceTests : ServiceTest<UserHearthbeatService>
{
    protected override UserHearthbeatService CreateService()
    {
        return new UserHearthbeatService(DatabaseBuilder);
    }

    private async Task InitDataAsync()
    {
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

        await Repository.AddAsync(Database.Entity.User.FromDiscord(user));
        await Repository.CommitAsync();
    }

    private static ApiRequestContext CreateContext(bool isPublic)
    {
        return new ApiRequestContext
        {
            LoggedUserData = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, !isPublic ? "Admin" : "User"),
                new Claim(ClaimTypes.NameIdentifier, Consts.UserId.ToString())
            })),
            LoggedUser = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build()
        };
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task UpdateHearthbeatAsync_UserNotFound()
    {
        var context = CreateContext(false);
        await Service.UpdateHearthbeatAsync(true, context);
    }

    [TestMethod]
    public async Task UpdateHearthbeatAsync_Active_PublicAdmin() => await ProcessTestAsync(true, true);

    [TestMethod]
    public async Task UpdateHearthbeatAsync_Active_PrivateAdmin() => await ProcessTestAsync(false, true);

    [TestMethod]
    public async Task UpdateHearthbeatAsync_Inactive_PublicAdmin() => await ProcessTestAsync(true, false);

    [TestMethod]
    public async Task UpdateHearthbeatAsync_Inactive_PrivateAdmin() => await ProcessTestAsync(false, false);

    private async Task ProcessTestAsync(bool isPublic, bool isActive)
    {
        await InitDataAsync();

        var context = CreateContext(isPublic);
        await Service.UpdateHearthbeatAsync(isActive, context);
    }
}
