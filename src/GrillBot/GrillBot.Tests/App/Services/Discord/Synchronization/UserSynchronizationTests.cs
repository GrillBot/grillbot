using GrillBot.App.Services.Discord.Synchronization;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;

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
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

        await Service.UserUpdatedAsync(user, user);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task UserUpdatedAsync_Ok()
    {
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

        await DbContext.Users.AddAsync(Database.Entity.User.FromDiscord(user));
        await DbContext.SaveChangesAsync();

        await Service.UserUpdatedAsync(user, user);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task UserUpdatedAsync_Bot()
    {
        var selfUser = new SelfUserBuilder()
            .SetId(Consts.UserId).SetUsername(Consts.Username).SetDiscriminator(Consts.Discriminator)
            .AsBot().Build();

        await DbContext.Users.AddAsync(Database.Entity.User.FromDiscord(selfUser));
        await DbContext.SaveChangesAsync();

        await Service.UserUpdatedAsync(selfUser, selfUser);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task InitBotAdminAsync_NewUser()
    {
        var owner = new UserBuilder()
            .SetId(Consts.UserId)
            .SetUsername(Consts.Username)
            .SetDiscriminator(Consts.Discriminator)
            .AsBot()
            .Build();

        var application = new ApplicationBuilder()
            .SetOwner(owner)
            .Build();

        await Service.InitBotAdminAsync(DbContext, application);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task InitBotAdminAsync_Exists()
    {
        var owner = new UserBuilder()
            .SetId(Consts.UserId)
            .SetUsername(Consts.Username)
            .SetDiscriminator(Consts.Discriminator)
            .AsBot()
            .Build();

        await DbContext.Users.AddAsync(Database.Entity.User.FromDiscord(owner));
        await DbContext.SaveChangesAsync();

        var application = new ApplicationBuilder()
            .SetOwner(owner)
            .Build();

        await Service.InitBotAdminAsync(DbContext, application);
        Assert.IsTrue(true);
    }
}
