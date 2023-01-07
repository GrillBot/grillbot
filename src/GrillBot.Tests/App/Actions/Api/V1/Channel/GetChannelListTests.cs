using Discord;
using GrillBot.App.Actions.Api.V1.Channel;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Channel;

[TestClass]
public class GetChannelListTests : ApiActionTest<GetChannelList>
{
    private IGuild Guild { get; set; }
    private ITextChannel TextChannel { get; set; }
    private IGuildUser User { get; set; }

    protected override GetChannelList CreateAction()
    {
        var guildBuilder = new GuildBuilder(Consts.GuildId, Consts.GuildName);

        TextChannel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(guildBuilder.Build()).Build();
        Guild = guildBuilder.SetGetTextChannelAction(TextChannel).Build();
        User = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(Guild).Build();

        var client = new ClientBuilder()
            .SetGetGuildAction(Guild)
            .Build();
        var messageCache = new MessageCacheBuilder()
            .Build();

        return new GetChannelList(ApiRequestContext, DatabaseBuilder, client, messageCache, TestServices.AutoMapper.Value);
    }

    [TestMethod]
    public async Task ProcessAsync_WithFilter()
    {
        var filter = new GetChannelListParams
        {
            ChannelType = ChannelType.Text,
            GuildId = Consts.GuildId.ToString(),
            NameContains = Consts.ChannelName[..5],
            HideDeleted = true
        };
        var result = await Action.ProcessAsync(filter);

        Assert.IsNotNull(result?.Data);
        Assert.AreEqual(0, result.TotalItemsCount);
    }

    [TestMethod]
    public async Task ProcessAsync_WithoutFilter()
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(Guild, User));
        await Repository.AddAsync(Database.Entity.GuildChannel.FromDiscord(TextChannel, ChannelType.Text));
        await Repository.AddAsync(new Database.Entity.GuildUserChannel
        {
            Count = 1, ChannelId = Consts.ChannelId.ToString(), GuildId = Consts.GuildId.ToString(), UserId = Consts.UserId.ToString(), FirstMessageAt = DateTime.MinValue,
            LastMessageAt = DateTime.MinValue
        });
        await Repository.CommitAsync();

        var filter = new GetChannelListParams();
        var result = await Action.ProcessAsync(filter);

        Assert.IsNotNull(result?.Data);
        Assert.AreEqual(1, result.TotalItemsCount);
    }
}
