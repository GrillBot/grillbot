using GrillBot.App.Services.Discord.Synchronization;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Services.Discord.Synchronization;

[TestClass]
public class UserSynchronizationTests : ServiceTest<UserSynchronization>
{
    protected override UserSynchronization CreateService()
    {
        return new UserSynchronization(DatabaseBuilder);
    }

    [TestMethod]
    public async Task UserUpdatedAsync_UserNotFound()
    {
        var user = new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

        await Service.UserUpdatedAsync(user);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task UserUpdatedAsync_Ok()
    {
        var user = new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

        await Repository.AddAsync(Database.Entity.User.FromDiscord(user));
        await Repository.CommitAsync();

        await Service.UserUpdatedAsync(user);
    }

    [TestMethod]
    public async Task UserUpdatedAsync_Bot()
    {
        var selfUser = new SelfUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).AsBot().Build();
        await Repository.AddAsync(Database.Entity.User.FromDiscord(selfUser));
        await Repository.CommitAsync();

        await Service.UserUpdatedAsync(selfUser);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task InitBotAdminAsync_NewUser()
    {
        var owner = new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).AsBot().Build();
        var application = new ApplicationBuilder().SetOwner(owner).Build();

        await UserSynchronization.InitBotAdminAsync(Repository, application);
    }

    [TestMethod]
    public async Task InitBotAdminAsync_Exists()
    {
        var owner = new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).AsBot().Build();
        await Repository.AddAsync(Database.Entity.User.FromDiscord(owner));
        await Repository.CommitAsync();

        var application = new ApplicationBuilder()
            .SetOwner(owner)
            .Build();

        await UserSynchronization.InitBotAdminAsync(Repository, application);
    }
}
