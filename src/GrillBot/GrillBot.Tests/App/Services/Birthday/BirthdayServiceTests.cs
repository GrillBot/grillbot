using GrillBot.App.Services.Birthday;
using GrillBot.Database.Services;
using System;

namespace GrillBot.Tests.App.Services.Birthday;

[TestClass]
public class BirthdayServiceTests : ServiceTest<BirthdayService>
{
    protected override BirthdayService CreateService()
    {
        var discordClient = DiscordHelper.CreateClient();
        return new BirthdayService(discordClient, DbFactory);
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
