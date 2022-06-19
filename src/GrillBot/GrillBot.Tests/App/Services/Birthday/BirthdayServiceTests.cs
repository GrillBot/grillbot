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
        return new BirthdayService(discordClient, DatabaseBuilder);
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
        await Repository.User.GetOrCreateUserAsync(user);
        await Repository.CommitAsync();

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
        await Repository.User.GetOrCreateUserAsync(user);
        await Repository.CommitAsync();

        await Service.RemoveBirthdayAsync(user);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task HaveBirthdayAsync_Yes()
    {
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var dbUser = Database.Entity.User.FromDiscord(user);
        dbUser.Birthday = DateTime.MaxValue;
        await Repository.AddAsync(dbUser);
        await Repository.CommitAsync();

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
        await Repository.AddAsync(dbUser);
        await Repository.CommitAsync();

        var result = await Service.GetTodayBirthdaysAsync();
        Assert.AreEqual(0, result.Count);
    }
}
