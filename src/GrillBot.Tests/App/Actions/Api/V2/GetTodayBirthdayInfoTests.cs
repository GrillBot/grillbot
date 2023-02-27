using System.Linq;
using Discord;
using GrillBot.App.Actions.Api.V2;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V2;

[TestClass]
public class GetTodayBirthdayInfoTests : ApiActionTest<GetTodayBirthdayInfo>
{
    private static IConfiguration Configuration => TestServices.Configuration.Value;
    private IUser[] Users { get; set; }

    protected override GetTodayBirthdayInfo CreateInstance()
    {
        Users = new[]
        {
            new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build(),
            new UserBuilder(Consts.UserId + 1, Consts.Username, Consts.Discriminator).Build()
        };

        var client = new ClientBuilder()
            .SetGetGuildsAction(Enumerable.Empty<IGuild>())
            .SetGetUserAction(Users)
            .Build();

        return new GetTodayBirthdayInfo(ApiRequestContext, DatabaseBuilder, client, Configuration, TestServices.Texts.Value);
    }

    [TestMethod]
    public async Task ProcessAsync_NoOneHaveBirthday()
    {
        var result = await Instance.ProcessAsync();
        var sadge = Configuration["Discord:Emotes:Sadge"];

        Assert.IsFalse(string.IsNullOrEmpty(result));
        Assert.IsTrue(result.Contains(sadge));
    }

    [TestMethod]
    public async Task ProcessAsync_HaveTodayBirthdays()
    {
        var now = DateTime.Now;

        var withYear = Database.Entity.User.FromDiscord(Users[1]);
        withYear.Birthday = now.Date;

        var withoutYear = Database.Entity.User.FromDiscord(Users[0]);
        withoutYear.Birthday = new DateTime(0001, now.Month, now.Day);

        var unknownUser = Database.Entity.User.FromDiscord(new UserBuilder(Consts.UserId + 4, Consts.Username, Consts.Discriminator).Build());
        unknownUser.Birthday = new DateTime(0001, now.Month, now.Day);

        await Repository.AddCollectionAsync(new[] { withoutYear, withYear, unknownUser });
        await Repository.CommitAsync();

        var result = await Instance.ProcessAsync();
        var hypers = Configuration["Discord:Emotes:Hypers"];

        Assert.IsFalse(string.IsNullOrEmpty(result));
        Assert.IsTrue(result.Contains(hypers));
    }

    [TestMethod]
    public async Task ProcessAsync_OnlyOne()
    {
        var now = DateTime.Now;

        var withYear = Database.Entity.User.FromDiscord(Users[0]);
        withYear.Birthday = now.Date;

        await Repository.AddCollectionAsync(new[] { withYear });
        await Repository.CommitAsync();

        var result = await Instance.ProcessAsync();
        var hypers = Configuration["Discord:Emotes:Hypers"];

        Assert.IsFalse(string.IsNullOrEmpty(result));
        Assert.IsTrue(result.Contains(hypers));
    }
}
