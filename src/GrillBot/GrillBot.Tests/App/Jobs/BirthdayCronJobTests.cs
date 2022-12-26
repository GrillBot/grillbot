using Discord;
using GrillBot.App.Actions.Api.V2;
using GrillBot.App.Jobs;
using GrillBot.Common.Managers;
using GrillBot.Tests.Infrastructure.Discord;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Tests.App.Jobs;

[TestClass]
public class BirthdayCronJobTests : JobTest<BirthdayCronJob>
{
    private static readonly IUser User = new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

    protected override BirthdayCronJob CreateJob()
    {
        var channel = new TextChannelBuilder(Consts.ChannelId + 1, Consts.ChannelName).Build();
        var guild = new GuildBuilder(Consts.GuildId + 1, Consts.GuildName).SetGetTextChannelsAction(new[] { channel }).Build();

        var client = new ClientBuilder()
            .SetGetGuildsAction(new[] { guild })
            .SetGetUserAction(User)
            .Build();
        var context = new ApiRequestContext();
        var configuration = TestServices.Configuration.Value;
        var texts = new TextsBuilder()
            .AddText("BirthdayModule/Info/Parts/WithYears", "cs", "WithYears")
            .AddText("BirthdayModule/Info/Template/SingleForm", "cs", "SingleForm")
            .Build();
        var action = new GetTodayBirthdayInfo(context, DatabaseBuilder, client, configuration, texts);
        var provider = TestServices.InitializedProvider.Value;
        provider.GetRequiredService<InitManager>().Set(true);

        return new BirthdayCronJob(configuration, client, action, DatabaseBuilder, provider);
    }

    private async Task InitDataAsync()
    {
        var user = Database.Entity.User.FromDiscord(User);
        user.Birthday = DateTime.Today;

        await Repository.AddAsync(user);
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task Execute_NoOneHave()
    {
        var context = CreateContext();
        await Job.Execute(context);

        Assert.IsNull(context.Result);
    }

    [TestMethod]
    public async Task Execute_GuildNotFound()
    {
        await InitDataAsync();

        var context = CreateContext();
        await Job.Execute(context);

        Assert.IsNotNull(context.Result);
        Assert.IsInstanceOfType(context.Result, typeof(string));

        var result = (string)context.Result;
        Assert.IsTrue(result.Contains("Required guild", StringComparison.InvariantCultureIgnoreCase), result);
    }

    [TestMethod]
    public async Task Execute_ChannelNotFound()
    {
        await InitDataAsync();

        var oldGuild = TestServices.Configuration.Value["Birthday:Notifications:GuildId"];
        var context = CreateContext();

        try
        {
            TestServices.Configuration.Value["Birthday:Notifications:GuildId"] = (Consts.GuildId + 1).ToString();
            await Job.Execute(context);

            Assert.IsNotNull(context.Result);
            Assert.IsInstanceOfType(context.Result, typeof(string));

            var result = (string)context.Result;
            Assert.IsTrue(result.Contains("Required channel", StringComparison.CurrentCultureIgnoreCase), result);
        }
        finally
        {
            TestServices.Configuration.Value["Birthday:Notifications:GuildId"] = oldGuild;
        }
    }

    [TestMethod]
    public async Task Execute_Success()
    {
        await InitDataAsync();

        var oldGuild = TestServices.Configuration.Value["Birthday:Notifications:GuildId"];
        var oldChannel = TestServices.Configuration.Value["Birthday:Notifications:ChannelId"];
        var context = CreateContext();

        try
        {
            TestServices.Configuration.Value["Birthday:Notifications:GuildId"] = (Consts.GuildId + 1).ToString();
            TestServices.Configuration.Value["Birthday:Notifications:ChannelId"] = (Consts.ChannelId + 1).ToString();
            await Job.Execute(context);

            Assert.IsNotNull(context.Result);
            Assert.IsInstanceOfType(context.Result, typeof(string));

            var result = (string)context.Result;
            Assert.IsFalse(result.Contains("Required guild", StringComparison.CurrentCultureIgnoreCase), result);
            Assert.IsFalse(result.Contains("Required channel", StringComparison.CurrentCultureIgnoreCase), result);
        }
        finally
        {
            TestServices.Configuration.Value["Birthday:Notifications:GuildId"] = oldGuild;
            TestServices.Configuration.Value["Birthday:Notifications:ChannelId"] = oldChannel;
        }
    }
}
