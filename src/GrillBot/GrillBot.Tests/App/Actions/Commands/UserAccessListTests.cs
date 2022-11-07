using System.Linq;
using Discord;
using GrillBot.App.Actions.Commands;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands;

[TestClass]
public class UserAccessListTests : CommandActionTest<UserAccessList>
{
    private static readonly ICategoryChannel Category = new CategoryBuilder().Build();
    private static readonly IGuild EmptyGuild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
    private static readonly Overwrite Overwrite = new(Consts.UserId + 3, PermissionTarget.User, new OverwritePermissions(0, int.MaxValue));

    private static readonly IGuildUser GuildUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetRoles(Enumerable.Empty<IRole>()).SetGuild(EmptyGuild)
        .Build();

    private static readonly ITextChannel[] Channels = Enumerable
        .Repeat(new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetPermissions(new[] { Overwrite }).SetGetUsersAction(new[] { GuildUser }).SetCategory(Category).Build(),
            15 * 25).Concat(new[] { new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGetUsersAction(new[] { GuildUser }).SetPermissions(new[] { Overwrite }).Build() }).ToArray();

    private static readonly IGuildUser UserWithoutChannels = new GuildUserBuilder(Consts.UserId + 1, Consts.Username, Consts.Discriminator).SetRoles(Enumerable.Empty<IRole>()).SetGuild(EmptyGuild).Build();

    private static readonly IGuild GuildData = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetChannelsAction(Channels).Build();

    protected override IMessageChannel Channel => Channels[0];
    protected override IGuild Guild => GuildData;
    protected override IGuildUser User => GuildUser;

    protected override UserAccessList CreateAction()
    {
        var texts = new TextsBuilder()
            .AddText("User/AccessList/Title", "en-US", "Title")
            .AddText("User/AccessList/NoAccess", "en-US", "NoAccess")
            .AddText("User/AccessList/WithoutCategory", "en-US", "WithoutCategory")
            .Build();

        return InitAction(new UserAccessList(texts));
    }

    [TestMethod]
    public async Task ProcessAsync()
    {
        var (embed, pagination) = await Action.ProcessAsync(GuildUser, 0);

        Assert.IsNotNull(embed);
        Assert.IsNotNull(pagination);
    }

    [TestMethod]
    public async Task ProcessAsync_WithoutChannels()
    {
        var (embed, pagination) = await Action.ProcessAsync(UserWithoutChannels, 0);

        Assert.IsNotNull(embed);
        Assert.IsNull(pagination);
    }

    [TestMethod]
    public async Task ComputePagesCount()
    {
        var result = await Action.ComputePagesCount(GuildUser);
        Assert.AreEqual(2, result);
    }
}
