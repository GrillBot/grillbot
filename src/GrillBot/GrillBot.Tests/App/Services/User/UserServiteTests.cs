using GrillBot.App.Services.User;
using GrillBot.Database.Enums;
using GrillBot.Database.Services;
using GrillBot.Tests.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Services.User;

[TestClass]
public class UserServiteTests : ServiceTest<UserService>
{
    protected override UserService CreateService()
    {
        var dbFactory = new DbContextBuilder();
        DbContext = dbFactory.Create();

        return new UserService(dbFactory);
    }

    public override void Cleanup()
    {
        DbContext.Users.RemoveRange(DbContext.Users.AsEnumerable());
        DbContext.SaveChanges();
    }

    [TestMethod]
    public async Task IsUserBotAdmin_NotFound()
    {
        var dcUser = DataHelper.CreateDiscordUser();
        var result = await Service.IsUserBotAdminAsync(dcUser);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task IsUserBotAdmin_Found_NotAdmin()
    {
        var dcUser = DataHelper.CreateDiscordUser(id: 654321);
        await DbContext.InitUserAsync(dcUser, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        var result = await Service.IsUserBotAdminAsync(dcUser);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task IsUserBotAdmin_Found_Admin()
    {
        var dcUser = DataHelper.CreateDiscordUser(id: 1234556);
        var userEntity = Database.Entity.User.FromDiscord(dcUser);
        userEntity.Flags |= (int)UserFlags.BotAdmin;

        await DbContext.Users.AddAsync(userEntity);
        await DbContext.SaveChangesAsync();

        var result = await Service.IsUserBotAdminAsync(dcUser);
        Assert.IsTrue(result);
    }
}
