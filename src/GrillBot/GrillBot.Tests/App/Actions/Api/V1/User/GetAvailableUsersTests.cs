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

    protected override GetAvailableUsers CreateAction()
    {
        var guildBuilder = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName);
        User = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guildBuilder.Build()).Build();
        Guild = guildBuilder.SetGetUsersAction(new[] { User }).Build();
        var client = new ClientBuilder().SetGetGuildsAction(new[] { Guild }).Build();

        return new GetAvailableUsers(ApiRequestContext, client, DatabaseBuilder);
    }

    [TestMethod]
    public async Task ProcessAsync_All()
    {
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.CommitAsync();

        var result = await Action.ProcessAsync(null);
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task ProcessAsync_Bots()
    {
        var result = await Action.ProcessAsync(true);
        Assert.AreEqual(0, result.Count);
    }
    
    [TestMethod]
    public async Task ProcessAsync_Users()
    {
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.CommitAsync();

        var result = await Action.ProcessAsync(false);
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    [ApiConfiguration(true)]
    public async Task ProcessAsync_AsPublic()
    {
        var result = await Action.ProcessAsync(null);
        Assert.AreEqual(0, result.Count);
    }
}
