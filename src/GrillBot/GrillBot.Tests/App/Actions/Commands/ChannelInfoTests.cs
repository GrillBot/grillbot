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

    private static readonly IGuild EmptyGuild = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetTextChannelsAction(new[] { TextChannelWithoutDb }).Build();
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
        var texts = new TextsBuilder()
            .AddText("ChannelModule/ChannelInfo/NoAccess", "en-US", "NoAccess")
            .AddText("ChannelModule/ChannelInfo/CreatedAt", "en-US", "CreatedAt")
            .AddText("ChannelModule/ChannelInfo/TextChannelTitle", "en-US", "TextChannelTitle")
            .AddText("ChannelModule/ChannelInfo/MemberCountValue/One", "en-US", "One")
            .AddText("ChannelModule/ChannelInfo/MemberCountValue/FiveAndMore", "en-US", "FiveAndMore")
            .AddText("ChannelModule/ChannelInfo/MemberCount", "en-US", "MemberCount")
            .AddText("ChannelModule/ChannelInfo/PermsCountValue/FiveAndMore", "en-US", "Zero")
            .AddText("ChannelModule/ChannelInfo/PermsCountValue/One", "en-US", "One")
            .AddText("ChannelModule/ChannelInfo/PermsCount", "en-US", "PermsCount")
            .AddText("ChannelModule/ChannelInfo/PermsCountTitle", "en-US", "PermsCountTitle")
            .AddText("ChannelModule/ChannelInfo/MessageCountValue/One", "en-US", "One")
            .AddText("ChannelModule/ChannelInfo/MessageCountValue/FiveAndMore", "en-US", "FiveAndMore")
            .AddText("ChannelModule/ChannelInfo/MessageCount", "en-US", "MessageCount")
            .AddText("ChannelModule/ChannelInfo/FirstMessage", "en-US", "FirstMessage")
            .AddText("ChannelModule/ChannelInfo/LastMessage", "en-US", "LastMessage")
            .AddText("ChannelModule/ChannelInfo/TopTen", "en-US", "TopTen")
            .AddText("ChannelModule/ChannelInfo/Configuration", "en-US", "Configuration")
            .AddText("ChannelModule/ChannelInfo/Flags/CommandsDisabled", "en-US", "CommandsDisabled")
            .AddText("ChannelModule/ChannelInfo/Flags/AutoReplyDeactivated", "en-US", "AutoReplyDeactivated")
            .AddText("ChannelModule/ChannelInfo/Flags/StatsHidden", "en-US", "StatsHidden")
            .AddText("ChannelModule/ChannelInfo/Channel", "en-US", "Channel")
            .AddText("ChannelModule/ChannelInfo/TagsCountValue/FiveAndMore", "en-US", "FiveAndMore")
            .AddText("ChannelModule/ChannelInfo/TagsCount", "en-US", "TagsCount")
            .AddText("ChannelModule/ChannelInfo/PublicThreadCountValue/One", "en-US", "One")
            .AddText("ChannelModule/ChannelInfo/PrivateThreadCountValue/One", "en-US", "One")
            .AddText("ChannelModule/ChannelInfo/ThreadCount", "en-US", "ThreadCount")
            .Build();
        var formatHelper = new FormatHelper(texts);

        return InitAction(new ChannelInfo(texts, formatHelper, DatabaseBuilder));
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(EmptyGuild));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(GuildUser.FromDiscord(EmptyGuild, User));

        var channel = GuildChannel.FromDiscord(TextChannelWithDb, ChannelType.Text);
        channel.Flags = (long)(ChannelFlags.CommandsDisabled | ChannelFlags.AutoReplyDeactivated | ChannelFlags.PointsDeactivated);
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
        channelWithDisabledStats.Flags = (long)ChannelFlags.StatsHidden;
        await Repository.AddAsync(channelWithDisabledStats);

        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_WithoutAccess()
    {
        var result = await Action.ProcessAsync(SecretChannel);

        Assert.IsNull(result);
        Assert.IsFalse(Action.IsOk);
        Assert.IsFalse(string.IsNullOrEmpty(Action.ErrorMessage));
    }

    [TestMethod]
    public async Task ProcessAsync_TextChannel_WithoutDb()
    {
        var result = await Action.ProcessAsync(TextChannelWithoutDb);

        CheckValidEmbed(result);
        Assert.IsTrue(Action.IsOk);
        Assert.IsTrue(string.IsNullOrEmpty(Action.ErrorMessage));
    }

    [TestMethod]
    public async Task ProcessAsync_TextChannel_WithDb()
    {
        await InitDataAsync();
        var result = await Action.ProcessAsync(TextChannelWithDb);

        CheckValidEmbed(result);
        Assert.IsTrue(Action.IsOk);
        Assert.IsTrue(string.IsNullOrEmpty(Action.ErrorMessage));
    }

    [TestMethod]
    public async Task ProcessAsync_TextChannel_WithDbAndDisabledStats()
    {
        await InitDataAsync();
        var result = await Action.ProcessAsync(TextChannelWithDbAndDisabledStats);

        CheckValidEmbed(result);
        Assert.IsTrue(Action.IsOk);
        Assert.IsTrue(string.IsNullOrEmpty(Action.ErrorMessage));
    }

    [TestMethod]
    public async Task ProcessAsync_Thread()
    {
        var result = await Action.ProcessAsync(Thread);

        CheckValidEmbed(result);
        Assert.IsTrue(Action.IsOk);
        Assert.IsTrue(string.IsNullOrEmpty(Action.ErrorMessage));
    }

    [TestMethod]
    public async Task ProcessAsync_Forum()
    {
        var result = await Action.ProcessAsync(Forum);

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
