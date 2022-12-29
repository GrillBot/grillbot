using Discord;
using GrillBot.App.Actions.Commands.Searching;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands.Searching;

[TestClass]
public class GetSearchingListTests : CommandActionTest<GetSearchingList>
{
    private static readonly IGuild GuildData = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();

    protected override IMessageChannel Channel => new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(GuildData).Build();
    protected override IDiscordInteraction Interaction => new DiscordInteractionBuilder(Consts.InteractionId).Build();
    protected override IGuild Guild => GuildData;
    protected override IGuildUser User => new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(GuildData).Build();

    protected override GetSearchingList CreateAction()
    {
        var apiContext = new ApiRequestContext();
        var client = new ClientBuilder().Build();
        var apiAction = new GrillBot.App.Actions.Api.V1.Searching.GetSearchingList(apiContext, client, DatabaseBuilder, TestServices.AutoMapper.Value);
        return InitAction(new GetSearchingList(apiAction, DatabaseBuilder, TestServices.Texts.Value));
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(Guild, User));
        await Repository.AddAsync(Database.Entity.GuildChannel.FromDiscord((ITextChannel)Channel, ChannelType.Text));

        await Repository.AddCollectionAsync(new[]
        {
            new Database.Entity.SearchItem
            {
                Id = 1,
                ChannelId = Consts.ChannelId.ToString(),
                GuildId = Consts.GuildId.ToString(),
                MessageContent = "\"Test\"",
                UserId = Consts.UserId.ToString()
            },
            new Database.Entity.SearchItem
            {
                Id = 2,
                ChannelId = Consts.ChannelId.ToString(),
                GuildId = Consts.GuildId.ToString(),
                MessageContent = "Test",
                UserId = Consts.UserId.ToString()
            }
        });

        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_NoItems()
    {
        var result = await Action.ProcessAsync(0, null, null);

        Assert.IsNotNull(result.embed);
        Assert.IsNull(result.paginationComponent);
    }

    [TestMethod]
    public async Task ProcessAsync_NoItemsWithQuery()
    {
        var result = await Action.ProcessAsync(0, "Search", null);

        Assert.IsNotNull(result.embed);
        Assert.IsNull(result.paginationComponent);
        Assert.AreEqual(0, result.embed.Fields.Length);
    }

    [TestMethod]
    public async Task ProcessAsync()
    {
        await InitDataAsync();
        var result = await Action.ProcessAsync(0, null, null);

        Assert.IsNotNull(result.embed);
        Assert.IsNull(result.paginationComponent);
        Assert.AreEqual(2, result.embed.Fields.Length);
    }

    [TestMethod]
    public async Task ComputePagesCountAsync()
    {
        var result = await Action.ComputePagesCountAsync(null, Channel);

        Assert.AreEqual(0, result);
    }
}
