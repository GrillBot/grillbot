using Discord;
using GrillBot.App.Actions.Commands.Searching;
using GrillBot.App.Managers;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands.Searching;

[TestClass]
public class GetSuggestionsTests : CommandActionTest<GetSuggestions>
{
    private static readonly IGuild GuildData = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();

    protected override IGuildUser User => new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
    protected override IMessageChannel Channel => new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(GuildData).Build();
    protected override IGuild Guild => GuildData;

    protected override GetSuggestions CreateInstance()
    {
        var userManager = new UserManager(DatabaseBuilder);
        var apiContext = new ApiRequestContext();
        var client = new ClientBuilder().Build();
        var apiAction = new GrillBot.App.Actions.Api.V1.Searching.GetSearchingList(apiContext, client, DatabaseBuilder, TestServices.AutoMapper.Value);
        return InitAction(new GetSuggestions(userManager, apiAction));
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(GuildData));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(GuildData, User));
        await Repository.AddAsync(Database.Entity.GuildChannel.FromDiscord((IGuildChannel)Channel, ChannelType.Text));
        await Repository.AddAsync(new Database.Entity.SearchItem
        {
            ChannelId = Consts.ChannelId.ToString(),
            UserId = Consts.UserId.ToString(),
            GuildId = Consts.GuildId.ToString(),
            Id = 1,
            MessageContent = "Test"
        });
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync()
    {
        await InitDataAsync();

        var result = await Instance.ProcessAsync();

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(1L, result[0].Value);
    }
}
