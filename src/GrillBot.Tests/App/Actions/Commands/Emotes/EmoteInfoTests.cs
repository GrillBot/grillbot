using Discord;
using GrillBot.App.Actions.Commands.Emotes;
using GrillBot.Common;
using GrillBot.Common.Helpers;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands.Emotes;

[TestClass]
public class EmoteInfoTests : CommandActionTest<EmoteInfo>
{
    protected override IGuild Guild
        => new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();

    protected override IGuildUser User
        => new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

    protected override EmoteInfo CreateInstance()
    {
        var client = new ClientBuilder().SetGetGuildsAction(new[] { Guild }).Build();
        var formatHelper = new FormatHelper(TestServices.Texts.Value);

        return InitAction(new EmoteInfo(DatabaseBuilder, client, TestServices.Texts.Value, formatHelper));
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(new Database.Entity.EmoteStatisticItem
        {
            EmoteId = Consts.OnlineEmoteId,
            FirstOccurence = DateTime.MinValue,
            Guild = Database.Entity.Guild.FromDiscord(Guild),
            GuildId = Guild.Id.ToString(),
            LastOccurence = DateTime.MaxValue,
            UseCount = 1,
            User = Database.Entity.GuildUser.FromDiscord(Guild, User),
            UserId = User.Id.ToString()
        });
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_UnicodeEmoji()
    {
        var result = await Instance.ProcessAsync(Emojis.Ok);

        Assert.IsNull(result);
        Assert.IsFalse(Instance.IsOk);
        Assert.IsNotNull(Instance.ErrorMessage);
    }

    [TestMethod]
    public async Task ProcessAsync_NoStatistics()
    {
        var result = await Instance.ProcessAsync(Consts.OnlineEmote);

        Assert.IsNull(result);
        Assert.IsFalse(Instance.IsOk);
        Assert.IsNotNull(Instance.ErrorMessage);
    }

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        await InitDataAsync();

        var result = await Instance.ProcessAsync(Consts.OnlineEmote);

        Assert.IsNotNull(result);
        Assert.IsTrue(Instance.IsOk);
        Assert.IsNull(Instance.ErrorMessage);
    }
}
