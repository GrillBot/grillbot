using GrillBot.App.Services.Birthday;
using GrillBot.Database.Services;
using GrillBot.Tests.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Services.Birthday;

[TestClass]
public class BirthdayServiceTests : ServiceTest<BirthdayService>
{
    protected override BirthdayService CreateService()
    {
        var discordClient = DiscordHelper.CreateClient();
        var dbFactory = new DbContextBuilder();
        DbContext = dbFactory.Create();

        return new BirthdayService(discordClient, dbFactory);
    }

    public override void Cleanup()
    {
        DbContext.ChangeTracker.Clear();
        DbContext.Users.RemoveRange(DbContext.Users.AsEnumerable());
        DbContext.SaveChanges();
    }

    [TestMethod]
    public async Task AddBirthayAsync_WithInit()
    {
        var user = DataHelper.CreateDiscordUser();
        await Service.AddBirthdayAsync(user, new DateTime(2022, 02, 04));
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task AddBirthdayAsync_WithoutInit()
    {
        var user = DataHelper.CreateDiscordUser();
        await DbContext.InitUserAsync(user, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        await Service.AddBirthdayAsync(user, new DateTime(2022, 02, 04));
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task RemoveBirthdayAsync_NotFound()
    {
        var user = DataHelper.CreateDiscordUser();
        await Service.RemoveBirthdayAsync(user);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task RemoveBirthdayAsync_Found()
    {
        var user = DataHelper.CreateDiscordUser();
        await DbContext.InitUserAsync(user, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        await Service.RemoveBirthdayAsync(user);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task HaveBirthdayAsync_Yes()
    {
        await DbContext.Users.AddAsync(new Database.Entity.User() { Birthday = new(2022, 02, 04), Discriminator = "1234", Id = "12345", Username = "User" });
        await DbContext.SaveChangesAsync();

        var user = DataHelper.CreateDiscordUser();
        var result = await Service.HaveBirthdayAsync(user);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task HaveBirthdayAsync_No()
    {
        var user = DataHelper.CreateDiscordUser();
        var result = await Service.HaveBirthdayAsync(user);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task GetTodayBirthdaysAsync()
    {
        await DbContext.Users.AddAsync(new Database.Entity.User() { Birthday = DateTime.Today, Discriminator = "1234", Id = "12345", Username = "User" });
        await DbContext.SaveChangesAsync();

        var result = await Service.GetTodayBirthdaysAsync();
        Assert.AreEqual(0, result.Count);
    }
}
