using GrillBot.App.Services.Birthday;
using GrillBot.Tests.Infrastructure.Discord;
using Discord;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Services.Birthday;

[TestClass]
public class BirthdayServiceTests : ServiceTest<BirthdayService>
{
    private static IUser User { get; set; }

    protected override BirthdayService CreateService()
    {
        User = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var discordClient = new ClientBuilder()
            .SetGetUserAction(User)
            .Build();

        return new BirthdayService(discordClient, DatabaseBuilder);
    }

    [TestMethod]
    public async Task AddBirthayAsync_WithInit()
    {
        await Service.AddBirthdayAsync(User, new DateTime(2022, 02, 04));
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task AddBirthdayAsync_WithoutInit()
    {
        await Repository.User.GetOrCreateUserAsync(User);
        await Repository.CommitAsync();

        await Service.AddBirthdayAsync(User, new DateTime(2022, 02, 04));
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task RemoveBirthdayAsync_NotFound()
    {
        await Service.RemoveBirthdayAsync(User);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task RemoveBirthdayAsync_Found()
    {
        await Repository.User.GetOrCreateUserAsync(User);
        await Repository.CommitAsync();

        await Service.RemoveBirthdayAsync(User);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task HaveBirthdayAsync_Yes()
    {
        var dbUser = Database.Entity.User.FromDiscord(User);
        dbUser.Birthday = DateTime.MaxValue;
        await Repository.AddAsync(dbUser);
        await Repository.CommitAsync();

        var result = await Service.HaveBirthdayAsync(User);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task HaveBirthdayAsync_No()
    {
        var result = await Service.HaveBirthdayAsync(User);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task GetTodayBirthdaysAsync()
    {
        var dbUser = Database.Entity.User.FromDiscord(User);
        dbUser.Birthday = DateTime.Today;
        await Repository.AddAsync(dbUser);
        await Repository.CommitAsync();

        var result = await Service.GetTodayBirthdaysAsync();
        Assert.AreEqual(1, result.Count);
    }
}
