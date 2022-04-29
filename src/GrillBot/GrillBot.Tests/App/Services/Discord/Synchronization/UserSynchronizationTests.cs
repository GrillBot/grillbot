using GrillBot.App.Services.Discord.Synchronization;

namespace GrillBot.Tests.App.Services.Discord.Synchronization;

[TestClass]
public class UserSynchronizationTests : ServiceTest<UserSynchronization>
{
    protected override UserSynchronization CreateService()
    {
        return new UserSynchronization(DbFactory);
    }

    [TestMethod]
    public async Task UserUpdatedAsync_UserNotFound()
    {
        var user = DataHelper.CreateDiscordUser();

        await Service.UserUpdatedAsync(user, user);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task UserUpdatedAsync_Ok()
    {
        var user = DataHelper.CreateDiscordUser();

        await DbContext.Users.AddAsync(Database.Entity.User.FromDiscord(user));
        await DbContext.SaveChangesAsync();

        await Service.UserUpdatedAsync(user, user);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task UserUpdatedAsync_Bot()
    {
        var user = DataHelper.CreateSelfUser();

        await DbContext.Users.AddAsync(Database.Entity.User.FromDiscord(user));
        await DbContext.SaveChangesAsync();

        await Service.UserUpdatedAsync(user, user);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task InitBotAdminAsync_NewUser()
    {
        var application = DataHelper.CreateApplication();

        await Service.InitBotAdminAsync(DbContext, application);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task InitBotAdminAsync_Exists()
    {
        var application = DataHelper.CreateApplication();

        await DbContext.Users.AddAsync(Database.Entity.User.FromDiscord(application.Owner));
        await DbContext.SaveChangesAsync();

        await Service.InitBotAdminAsync(DbContext, application);
        Assert.IsTrue(true);
    }
}
