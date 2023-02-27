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
        = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetTextChannelsAction(new[] { TextChannel }).Build();

    protected override IMessageChannel Channel => TextChannel;
    protected override IGuildUser User => GuildUser;

    protected override GetChannelboard CreateInstance()
    {
        var texts = TestServices.Texts.Value;
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
        var (embed, pagination) = await Instance.ProcessAsync(0);

        Assert.IsNotNull(embed);
        Assert.IsNull(pagination);
    }

    [TestMethod]
    public async Task ProcessAsync_NoActivity()
    {
        var (embed, pagination) = await Instance.ProcessAsync(0);

        Assert.IsNotNull(embed);
        Assert.IsNull(pagination);
        Assert.AreNotEqual("Row", embed.Description);
    }

    [TestMethod]
    public async Task ComputePagesCount()
    {
        await InitDataAsync();
        var result = await Instance.ComputePagesCountAsync();

        Assert.AreEqual(1, result);
    }
}
