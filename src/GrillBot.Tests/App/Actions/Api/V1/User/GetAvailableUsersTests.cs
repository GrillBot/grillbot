using Discord;
using GrillBot.App.Actions.Api.V1.User;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Common.Attributes;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.User;

[TestClass]
public class GetAvailableUsersTests : ApiActionTest<GetAvailableUsers>
{
    private IGuild Guild { get; set; }
    private IGuildUser User { get; set; }

    protected override GetAvailableUsers CreateInstance()
    {
        var guildBuilder = new GuildBuilder(Consts.GuildId, Consts.GuildName);
        User = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guildBuilder.Build()).Build();
        Guild = guildBuilder.SetGetUsersAction(new[] { User }).Build();
        var client = new ClientBuilder().SetGetGuildsAction(new[] { Guild }).Build();

        return new GetAvailableUsers(ApiRequestContext, client, DatabaseBuilder);
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(Guild, User));
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_All()
    {
        await InitDataAsync();

        var result = await Instance.ProcessAsync(null, null);
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task ProcessAsync_Bots()
    {
        var result = await Instance.ProcessAsync(true, null);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task ProcessAsync_Users()
    {
        await InitDataAsync();

        var result = await Instance.ProcessAsync(false, null);
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    [ApiConfiguration(true)]
    public async Task ProcessAsync_AsPublic()
    {
        var result = await Instance.ProcessAsync(null, null);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task ProcessAsync_WithGuild()
    {
        await InitDataAsync();

        var result = await Instance.ProcessAsync(null, Guild.Id);
        Assert.AreEqual(1, result.Count);
    }
}
