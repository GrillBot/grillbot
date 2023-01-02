using System.Linq;
using Discord;
using GrillBot.App.Actions.Commands;
using GrillBot.Common.Helpers;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands;

[TestClass]
public class ChannelInfoTests : CommandActionTest<ChannelInfo>
{
    private static readonly Overwrite[] AllowAll = { new(Consts.GuildId, PermissionTarget.Role, new OverwritePermissions(ulong.MaxValue, 0)) };
    private static readonly GuildUserBuilder DefaultUserBuilder = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetRoles(Enumerable.Empty<ulong>());

    private static readonly ITextChannel TextChannelWithoutDb = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGetUsersAction(new[] { DefaultUserBuilder.Build() })
        .SetPermissions(AllowAll).Build();

    private static readonly IGuild EmptyGuild = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetTextChannelsAction(new[] { TextChannelWithoutDb })
        .SetGetUsersAction(new[] { DefaultUserBuilder.Build() }).Build();

    private static readonly IGuildUser DefaultUser = DefaultUserBuilder.SetGuild(EmptyGuild).Build();
    private static readonly Overwrite[] DisabledPerms = { new(Consts.GuildId, PermissionTarget.Role, new OverwritePermissions(0, ulong.MaxValue)) };
    private static readonly IGuildChannel SecretChannel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(EmptyGuild).SetPermissions(DisabledPerms).Build();

    private static readonly IGuildChannel TextChannelWithDb = new TextChannelBuilder(Consts.ChannelId + 1, Consts.ChannelName).SetGetUsersAction(new[] { DefaultUser })
        .SetPermissions(AllowAll).SetGuild(EmptyGuild).Build();

    private static readonly IGuildChannel TextChannelWithDbAndDisabledStats = new TextChannelBuilder(Consts.ChannelId + 2, Consts.ChannelName).SetGetUsersAction(new[] { DefaultUser })
        .SetPermissions(AllowAll).SetGuild(EmptyGuild).Build();

    private static readonly IThreadChannel Thread = new ThreadBuilder(Consts.ThreadId, Consts.ThreadName).SetGuild(EmptyGuild).SetParentChannel(TextChannelWithoutDb).SetType(ThreadType.PrivateThread)
        .Build();

    private static readonly IForumChannel Forum = new ForumBuilder(Consts.ForumId, Consts.ForumName).SetPermissions(Enumerable.Empty<Overwrite>()).SetTags(Enumerable.Empty<ForumTag>())
        .SetActiveThreadsAction(new[]
        {
            new ThreadBuilder(Consts.ThreadId, Consts.ThreadName).SetGuild(EmptyGuild).SetParentChannel(new ForumBuilder(Consts.ForumId, Consts.ForumName).Build()).SetType(ThreadType.PrivateThread)
                .Build(),
            new ThreadBuilder(Consts.ThreadId, Consts.ThreadName).SetGuild(EmptyGuild).SetParentChannel(new ForumBuilder(Consts.ForumId, Consts.ForumName).Build()).SetType(ThreadType.PublicThread)
                .Build()
        }).SetTopic("Topic").Build();

    protected override IGuildUser User => DefaultUser;
    protected override IMessageChannel Channel => TextChannelWithoutDb;
    protected override IGuild Guild => EmptyGuild;

    protected override ChannelInfo CreateAction()
    {
        var texts = TestServices.Texts.Value;
        var formatHelper = new FormatHelper(texts);

        return InitAction(new ChannelInfo(texts, formatHelper, DatabaseBuilder));
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(EmptyGuild));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(GuildUser.FromDiscord(EmptyGuild, User));

        var channel = GuildChannel.FromDiscord(TextChannelWithDb, ChannelType.Text);
        channel.Flags = (long)(ChannelFlag.CommandsDisabled | ChannelFlag.AutoReplyDeactivated | ChannelFlag.EphemeralCommands | ChannelFlag.PointsDeactivated);
        channel.Users.Add(new GuildUserChannel
        {
            Count = 1,
            UserId = Consts.UserId.ToString(),
            FirstMessageAt = DateTime.Now,
            LastMessageAt = DateTime.Now,
            GuildId = Consts.GuildId.ToString()
        });

        await Repository.AddAsync(channel);

        var channelWithDisabledStats = GuildChannel.FromDiscord(TextChannelWithDbAndDisabledStats, ChannelType.Text);
        channelWithDisabledStats.Flags = (long)ChannelFlag.StatsHidden;
        await Repository.AddAsync(channelWithDisabledStats);

        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_WithoutAccess()
    {
        var result = await Action.ProcessAsync(SecretChannel, false);

        Assert.IsNull(result);
        Assert.IsFalse(Action.IsOk);
        Assert.IsFalse(string.IsNullOrEmpty(Action.ErrorMessage));
    }

    [TestMethod]
    public async Task ProcessAsync_TextChannel_WithoutDb()
    {
        var result = await Action.ProcessAsync(TextChannelWithoutDb, false);

        CheckValidEmbed(result);
        Assert.IsTrue(Action.IsOk);
        Assert.IsTrue(string.IsNullOrEmpty(Action.ErrorMessage));
    }

    [TestMethod]
    public async Task ProcessAsync_TextChannel_WithDb()
    {
        await InitDataAsync();
        var result = await Action.ProcessAsync(TextChannelWithDb, false);

        CheckValidEmbed(result);
        Assert.IsTrue(Action.IsOk);
        Assert.IsTrue(string.IsNullOrEmpty(Action.ErrorMessage));
    }

    [TestMethod]
    public async Task ProcessAsync_TextChannel_WithDbAndDisabledStats()
    {
        await InitDataAsync();
        var result = await Action.ProcessAsync(TextChannelWithDbAndDisabledStats, false);

        CheckValidEmbed(result);
        Assert.IsTrue(Action.IsOk);
        Assert.IsTrue(string.IsNullOrEmpty(Action.ErrorMessage));
    }

    [TestMethod]
    public async Task ProcessAsync_Thread()
    {
        var result = await Action.ProcessAsync(Thread, false);

        CheckValidEmbed(result);
        Assert.IsTrue(Action.IsOk);
        Assert.IsTrue(string.IsNullOrEmpty(Action.ErrorMessage));
    }

    [TestMethod]
    public async Task ProcessAsync_Forum()
    {
        var result = await Action.ProcessAsync(Forum, false);

        CheckValidEmbed(result);
        Assert.IsTrue(Action.IsOk);
        Assert.IsTrue(string.IsNullOrEmpty(Action.ErrorMessage));
    }

    private static void CheckValidEmbed(IEmbed embed)
    {
        Assert.IsNotNull(embed);
        Assert.IsTrue(embed.Fields.Length > 0);
        Assert.IsNotNull(embed.Footer);
        Assert.IsNotNull(embed.Author);
        Assert.IsNotNull(embed.Color);
        Assert.IsNotNull(embed.Timestamp);
    }
}
