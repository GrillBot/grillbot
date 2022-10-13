using GrillBot.App.Services.User;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Services.User;

[TestClass]
public class UserServiteTests : ServiceTest<UserService>
{
    protected override UserService CreateService()
    {
        return new UserService(DatabaseBuilder);
    }

    [TestMethod]
    public async Task CheckUserFlagsAsync_NotFound()
    {
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var result = await Service.CheckUserFlagsAsync(user, UserFlags.BotAdmin);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task CheckUserFlagsAsync_Found_NotAdmin()
    {
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        await Repository.User.GetOrCreateUserAsync(user);
        await Repository.CommitAsync();

        var result = await Service.CheckUserFlagsAsync(user, UserFlags.BotAdmin);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task CheckUserFlagsAsync_Found_Admin()
    {
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var userEntity = Database.Entity.User.FromDiscord(user);
        userEntity.Flags |= (int)UserFlags.BotAdmin;

        await Repository.AddAsync(userEntity);
        await Repository.CommitAsync();

        var result = await Service.CheckUserFlagsAsync(user, UserFlags.BotAdmin);
        Assert.IsTrue(result);
    }
}
