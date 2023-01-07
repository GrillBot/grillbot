using Discord;
using GrillBot.App.Actions.Api.V1.Channel;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Channel;

[TestClass]
public class GetChannelboardTests : ApiActionTest<GetChannelboard>
{
    private IGuild Guild { get; set; }
    private ITextChannel TextChannel { get; set; }
    private IGuildUser User { get; set; }

    protected override GetChannelboard CreateAction()
    {
        var guildBuilder = new GuildBuilder(Consts.GuildId, Consts.GuildName);

        TextChannel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(guildBuilder.Build()).Build();
        User = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guildBuilder.Build()).Build();
        Guild = guildBuilder.SetGetTextChannelsAction(new[] { TextChannel }).SetGetUserAction(User).Build();

        var discordClient = new ClientBuilder().SetGetUserAction(User).SetGetGuildsAction(new List<IGuild> { Guild }).Build();
        return new GetChannelboard(ApiRequestContext, discordClient, DatabaseBuilder, TestServices.AutoMapper.Value);
    }

    [TestMethod]
    public async Task ProcessAsync_WithoutStats()
    {
        var result = await Action.ProcessAsync();
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task Success()
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(Guild, User));
        await Repository.AddAsync(Database.Entity.GuildChannel.FromDiscord(TextChannel, ChannelType.Text));
        await Repository.AddAsync(new Database.Entity.GuildUserChannel
        {
            Count = 50, ChannelId = Consts.ChannelId.ToString(), GuildId = Consts.GuildId.ToString(), UserId = Consts.UserId.ToString(), FirstMessageAt = DateTime.Now, LastMessageAt = DateTime.Now
        });
        await Repository.CommitAsync();

        var result = await Action.ProcessAsync();
        Assert.AreEqual(1, result.Count);
    }
}
