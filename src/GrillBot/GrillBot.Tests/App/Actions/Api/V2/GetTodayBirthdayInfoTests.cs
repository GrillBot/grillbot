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
    private List<IUser> Users { get; set; }

    protected override GetTodayBirthdayInfo CreateAction()
    {
        Users = new List<IUser>
        {
            new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build(),
            new UserBuilder().SetIdentity(Consts.UserId + 1, Consts.Username, Consts.Discriminator).Build()
        };

        var clientBuilder = new ClientBuilder()
            .SetGetGuildsAction(Enumerable.Empty<IGuild>());
        foreach (var user in Users)
            clientBuilder.SetGetUserAction(user);
        var client = clientBuilder.Build();

        var texts = new TextsBuilder()
            .AddText("BirthdayModule/Info/NoOneHave", "cs", "NoOneHave {0}")
            .AddText("BirthdayModule/Info/Parts/WithoutYears", "cs", "{0}")
            .AddText("BirthdayModule/Info/Parts/WithYears", "cs", "{0},{1}")
            .AddText("BirthdayModule/Info/Template/MultipleForm", "cs", "{0},{1},{2}")
            .AddText("BirthdayModule/Info/Template/SingleForm", "cs", "{0},{1}")
            .Build();

        return new GetTodayBirthdayInfo(ApiRequestContext, DatabaseBuilder, client, Configuration, texts);
    }

    [TestMethod]
    public async Task ProcessAsync_NoOneHaveBirthday()
    {
        var result = await Action.ProcessAsync();
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

        var unknownUser = Database.Entity.User.FromDiscord(new UserBuilder().SetIdentity(Consts.UserId + 4, Consts.Username, Consts.Discriminator).Build());
        unknownUser.Birthday = new DateTime(0001, now.Month, now.Day);

        await Repository.AddCollectionAsync(new[] { withoutYear, withYear, unknownUser });
        await Repository.CommitAsync();

        var result = await Action.ProcessAsync();
        var hypers = Configuration["Discord:Emotes:Hypers"];

        Assert.IsFalse(string.IsNullOrEmpty(result));
        Assert.IsTrue(result.Contains(hypers));
    }

    [TestMethod]
    public async Task ProcessAsync_LastWithoutYear()
    {
        var now = DateTime.Now;

        var withYear = Database.Entity.User.FromDiscord(Users[0]);
        withYear.Birthday = now.Date;

        var withoutYear = Database.Entity.User.FromDiscord(Users[1]);
        withoutYear.Birthday = new DateTime(0001, now.Month, now.Day);

        var unknownUser = Database.Entity.User.FromDiscord(new UserBuilder().SetIdentity(Consts.UserId + 4, Consts.Username, Consts.Discriminator).Build());
        unknownUser.Birthday = new DateTime(0001, now.Month, now.Day);

        await Repository.AddCollectionAsync(new[] { withYear, withoutYear, unknownUser });
        await Repository.CommitAsync();

        var result = await Action.ProcessAsync();
        var hypers = Configuration["Discord:Emotes:Hypers"];

        Assert.IsFalse(string.IsNullOrEmpty(result));
        Assert.IsTrue(result.Contains(hypers));
    }
}
