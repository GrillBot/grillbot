using System.Linq;
using Discord;
using GrillBot.App.Actions.Commands;
using GrillBot.Common.Helpers;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands;

[TestClass]
public class GetChannelboardTests : CommandActionTest<GetChannelboard>
{
    private static readonly IGuild EmptyGuild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();

    private static readonly IGuildUser GuildUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetRoles(Enumerable.Empty<ulong>()).SetGuild(EmptyGuild)
        .SetGuildPermissions(GuildPermissions.All).Build();

    private static readonly ITextChannel TextChannel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGetUsersAction(new[] { GuildUser }).SetGuild(EmptyGuild)
        .SetPermissions(Enumerable.Empty<Overwrite>()).Build();

    protected override IGuild Guild { get; }
        = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetChannelsAction(new[] { TextChannel }).Build();

    protected override IMessageChannel Channel => TextChannel;
    protected override IGuildUser User => GuildUser;

    protected override GetChannelboard CreateAction()
    {
        var texts = new TextsBuilder()
            .AddText("ChannelModule/GetChannelboard/NoActivity", "en-US", "NoActivity")
            .AddText("ChannelModule/GetChannelboard/NoAccess", "en-US", "NoAccess")
            .AddText("ChannelModule/GetChannelboard/Title", "en-US", "Title")
            .AddText("ChannelModule/GetChannelboard/Row", "en-US", "Row")
            .AddText("ChannelModule/GetChannelboard/Counts/One", "en-US", "One")
            .AddText("ChannelModule/GetChannelboard/Counts/TwoToFour", "en-US", "TwoToFour")
            .AddText("ChannelModule/GetChannelboard/Counts/FiveAndMore", "en-US", "FiveAndMore")
            .Build();

        var formatHelper = new FormatHelper(texts);
        return InitAction(new GetChannelboard(DatabaseBuilder, texts, formatHelper));
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(Guild, User));
        await Repository.AddAsync(Database.Entity.GuildChannel.FromDiscord(TextChannel, ChannelType.Text));

        await Repository.AddAsync(new Database.Entity.GuildUserChannel
        {
            Count = 50,
            ChannelId = Consts.ChannelId.ToString(),
            GuildId = Consts.GuildId.ToString(),
            UserId = Consts.UserId.ToString(),
            FirstMessageAt = DateTime.MinValue,
            LastMessageAt = DateTime.MaxValue
        });
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync()
    {
        await InitDataAsync();
        var (embed, pagination) = await Action.ProcessAsync(0);

        Assert.IsNotNull(embed);
        Assert.IsNull(pagination);
        Assert.AreEqual("Row", embed.Description);
    }

    [TestMethod]
    public async Task ProcessAsync_NoActivity()
    {
        var (embed, pagination) = await Action.ProcessAsync(0);

        Assert.IsNotNull(embed);
        Assert.IsNull(pagination);
        Assert.AreNotEqual("Row", embed.Description);
    }

    [TestMethod]
    public async Task ComputePagesCount()
    {
        await InitDataAsync();
        var result = await Action.ComputePagesCountAsync();

        Assert.AreEqual(1, result);
    }
}
