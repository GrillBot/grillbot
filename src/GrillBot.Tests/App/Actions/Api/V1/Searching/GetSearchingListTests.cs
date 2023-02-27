using Discord;
using GrillBot.App.Actions.Api.V1.Searching;
using GrillBot.Data.Models.API.Searching;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Common.Attributes;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Searching;

[TestClass]
public class GetSearchingListTests : ApiActionTest<GetSearchingList>
{
    private IGuild Guild { get; set; }
    private IGuildUser User { get; set; }
    private ITextChannel TextChannel { get; set; }

    protected override GetSearchingList CreateInstance()
    {
        var guildBuilder = new GuildBuilder(Consts.GuildId, Consts.GuildName);
        User = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guildBuilder.Build()).Build();
        TextChannel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(guildBuilder.Build()).Build();
        Guild = guildBuilder.SetGetUsersAction(new[] { User }).SetGetTextChannelsAction(new[] { TextChannel }).Build();
        var client = new ClientBuilder().SetGetGuildsAction(new[] { Guild }).Build();

        return new GetSearchingList(ApiRequestContext, client, DatabaseBuilder, TestServices.AutoMapper.Value);
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(Guild, User));
        await Repository.AddAsync(Database.Entity.GuildChannel.FromDiscord(TextChannel, ChannelType.Text));
        await Repository.AddAsync(new Database.Entity.SearchItem
        {
            ChannelId = Consts.ChannelId.ToString(),
            GuildId = Consts.GuildId.ToString(),
            MessageContent = "Content",
            UserId = Consts.UserId.ToString()
        });
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_WithoutFilter()
    {
        await InitDataAsync();

        var filter = new GetSearchingListParams();
        var result = await Instance.ProcessAsync(filter);

        Assert.AreEqual(1, result.TotalItemsCount);
    }

    [TestMethod]
    public async Task ProcessAsync_WithFilter()
    {
        var filter = new GetSearchingListParams
        {
            ChannelId = Consts.ChannelId.ToString(),
            GuildId = Consts.GuildId.ToString(),
            MessageQuery = "Query",
            UserId = Consts.UserId.ToString()
        };

        var result = await Instance.ProcessAsync(filter);
        Assert.AreEqual(0, result.TotalItemsCount);
    }

    [TestMethod]
    [ApiConfiguration(true)]
    public async Task ProcessAsync_Public()
    {
        await InitDataAsync();

        var filter = new GetSearchingListParams();
        var result = await Instance.ProcessAsync(filter);
        Assert.AreEqual(1, result.TotalItemsCount);
    }

    [TestMethod]
    [ApiConfiguration(true)]
    public async Task ProcessAsync_UnallowedGuild()
    {
        await InitDataAsync();

        var filter = new GetSearchingListParams { GuildId = (Consts.GuildId + 1).ToString() };
        var result = await Instance.ProcessAsync(filter);
        Assert.AreEqual(1, result.TotalItemsCount);
    }
}
