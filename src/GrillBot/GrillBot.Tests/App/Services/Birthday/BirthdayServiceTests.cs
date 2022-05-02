using GrillBot.App.Services.Birthday;
using GrillBot.Database.Services;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;
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
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        await Service.AddBirthdayAsync(user, new DateTime(2022, 02, 04));
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task AddBirthdayAsync_WithoutInit()
    {
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        await DbContext.InitUserAsync(user, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        await Service.AddBirthdayAsync(user, new DateTime(2022, 02, 04));
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task RemoveBirthdayAsync_NotFound()
    {
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        await Service.RemoveBirthdayAsync(user);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task RemoveBirthdayAsync_Found()
    {
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        await DbContext.InitUserAsync(user, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        await Service.RemoveBirthdayAsync(user);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task HaveBirthdayAsync_Yes()
    {
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var dbUser = Database.Entity.User.FromDiscord(user);
        dbUser.Birthday = DateTime.MaxValue;
        await DbContext.AddAsync(dbUser);
        await DbContext.SaveChangesAsync();

        var result = await Service.HaveBirthdayAsync(user);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task HaveBirthdayAsync_No()
    {
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var result = await Service.HaveBirthdayAsync(user);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task GetTodayBirthdaysAsync()
    {
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var dbUser = Database.Entity.User.FromDiscord(user);
        dbUser.Birthday = DateTime.Today;
        await DbContext.Users.AddAsync(dbUser);
        await DbContext.SaveChangesAsync();

        var result = await Service.GetTodayBirthdaysAsync();
        Assert.AreEqual(0, result.Count);
    }
}
