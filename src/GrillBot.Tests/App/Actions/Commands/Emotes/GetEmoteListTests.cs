using Discord;
using GrillBot.App.Actions.Commands.Emotes;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands.Emotes;

[TestClass]
public class GetEmoteListTests : CommandActionTest<GetEmotesList>
{
    private static readonly GuildEmote Emote =
        ReflectionHelper.CreateWithInternalConstructor<GuildEmote>(Consts.OnlineEmote.Id, Consts.OnlineEmote.Name, Consts.OnlineEmote.Animated, false, false, null, null);

    private static readonly IGuild GuildData = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetEmotes(new[] { Emote }).Build();

    protected override IGuild Guild => GuildData;

    protected override IGuildUser User
        => new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(GuildData).Build();

    protected override GetEmotesList CreateAction()
    {
        var client = new ClientBuilder().SetGetGuildsAction(new[] { Guild }).Build();
        var emoteHelper = new GrillBot.App.Helpers.EmoteHelper(client);
        var apiAction = new GrillBot.App.Actions.Api.V1.Emote.GetStatsOfEmotes(new ApiRequestContext(), DatabaseBuilder, TestServices.AutoMapper.Value, emoteHelper);

        return InitAction(new GetEmotesList(apiAction, TestServices.Texts.Value));
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(Guild, User));
        await Repository.AddAsync(new Database.Entity.EmoteStatisticItem
        {
            UserId = Consts.UserId.ToString(),
            GuildId = Consts.GuildId.ToString(),
            EmoteId = Consts.OnlineEmoteId,
            FirstOccurence = DateTime.Now,
            LastOccurence = DateTime.Now,
            UseCount = 50
        });

        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_NoEmotes()
    {
        var (embed, paginationComponent) = await Action.ProcessAsync(0, "UseCount", true, null, false);

        Assert.IsNotNull(embed);
        Assert.IsNull(paginationComponent);
    }

    [TestMethod]
    public async Task ProcessAsync_NoEmotes_OfUser()
    {
        var (embed, paginationComponent) = await Action.ProcessAsync(0, "UseCount", false, User, false);

        Assert.IsNotNull(embed);
        Assert.IsNull(paginationComponent);
    }

    [TestMethod]
    public async Task ProcessAsync_WithEmotes()
    {
        await InitDataAsync();

        var (embed, paginationComponent) = await Action.ProcessAsync(0, "UseCount", false, User, false);
        Assert.IsNotNull(embed);
        Assert.IsNull(paginationComponent);
    }

    [TestMethod]
    public async Task ComputePagesCountAsync()
    {
        await InitDataAsync();

        var result = await Action.ComputePagesCountAsync("UseCount", false, null, false);
        Assert.AreEqual(1, result);
    }
}
