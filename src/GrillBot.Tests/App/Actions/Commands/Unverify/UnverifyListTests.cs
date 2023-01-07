using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Discord;
using GrillBot.App.Actions.Commands.Unverify;
using GrillBot.Common.Helpers;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models;
using GrillBot.Data.Models.Unverify;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;
using Newtonsoft.Json;

namespace GrillBot.Tests.App.Actions.Commands.Unverify;

[TestClass]
public class UnverifyListTests : CommandActionTest<UnverifyList>
{
    private static readonly IRole Role = new RoleBuilder(Consts.RoleId, Consts.RoleName).SetColor(Color.Green).Build();
    private static readonly IGuild EmptyGuild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
    private static readonly IGuildUser GuildUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(EmptyGuild).Build();

    private static readonly ITextChannel[] Channels =
    {
        new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(EmptyGuild).Build(),
        new TextChannelBuilder(Consts.ChannelId + 1, Consts.ChannelName).SetGuild(EmptyGuild).Build()
    };

    private static readonly IGuild GuildData = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetRoleAction(Role).SetGetUsersAction(new[] { GuildUser })
        .SetGetTextChannelsAction(Channels).Build();

    protected override IGuild Guild => GuildData;
    protected override IGuildUser User => GuildUser;

    protected override UnverifyList CreateAction()
    {
        var texts = TestServices.Texts.Value;
        var formatHelper = new FormatHelper(texts);
        return InitAction(new UnverifyList(DatabaseBuilder, texts, formatHelper));
    }

    private async Task InitDataAsync(bool isSelfunverify)
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(GuildData));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(Guild, User));

        if (isSelfunverify)
        {
            var hiddenStatsChannel = GuildChannel.FromDiscord(Channels[0], ChannelType.Text);
            hiddenStatsChannel.Flags |= (long)ChannelFlag.StatsHidden;
            await Repository.AddAsync(hiddenStatsChannel);
        }

        foreach (var channel in Channels.Skip(isSelfunverify ? 1 : 0))
            await Repository.AddAsync(GuildChannel.FromDiscord(channel, ChannelType.Text));

        await Repository.AddAsync(new Database.Entity.Unverify
        {
            Channels = new List<GuildChannelOverride> { new() { AllowValue = int.MaxValue, DenyValue = int.MaxValue, ChannelId = Consts.ChannelId } },
            Reason = isSelfunverify ? null : "Reason",
            Roles = new List<string> { Consts.RoleId.ToString() },
            EndAt = DateTime.MaxValue,
            GuildId = Consts.GuildId.ToString(),
            StartAt = DateTime.Now,
            UserId = Consts.UserId.ToString(),
            UnverifyLog = new UnverifyLog
            {
                Operation = isSelfunverify ? UnverifyOperation.Selfunverify : UnverifyOperation.Unverify,
                CreatedAt = DateTime.Now,
                GuildId = Consts.GuildId.ToString(),
                FromUserId = Consts.UserId.ToString(),
                ToUserId = Consts.UserId.ToString(),
                Id = 1,
                Data = JsonConvert.SerializeObject(new UnverifyLogSet
                {
                    End = DateTime.MaxValue,
                    Reason = isSelfunverify ? null : "Reason",
                    Start = DateTime.Now,
                    ChannelsToKeep = new List<ChannelOverride>
                    {
                        new() { AllowValue = int.MaxValue, DenyValue = int.MaxValue, ChannelId = Consts.ChannelId + 1 },
                        new() { AllowValue = int.MaxValue, DenyValue = int.MaxValue, ChannelId = Consts.ChannelId + 2 }
                    },
                    ChannelsToRemove = Enumerable.Repeat(new ChannelOverride { AllowValue = int.MaxValue, DenyValue = int.MaxValue, ChannelId = Consts.ChannelId }, 50).ToList(),
                    IsSelfUnverify = isSelfunverify,
                    RolesToKeep = new List<ulong> { Consts.RoleId + (isSelfunverify ? 1UL : 0UL) },
                    RolesToRemove = Enumerable.Repeat(Consts.RoleId, 50).ToList()
                })
            }
        });
        await Repository.CommitAsync();
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_NoUnverify() => await Action.ProcessAsync(0);

    [TestMethod]
    public async Task ProcessAsync_Unverify()
    {
        await InitDataAsync(false);
        var result = await Action.ProcessAsync(0);

        Assert.IsNotNull(result.embed);
        Assert.IsNull(result.paginationComponent);
        Assert.IsTrue(result.embed.Fields.Length > 0);
        Assert.IsNotNull(result.embed.Footer);
        Assert.IsNotNull(result.embed.Author);
    }

    [TestMethod]
    public async Task ProcessAsync_Selfunverify()
    {
        await InitDataAsync(true);
        var result = await Action.ProcessAsync(0);

        Assert.IsNotNull(result.embed);
        Assert.IsNull(result.paginationComponent);
        Assert.IsTrue(result.embed.Fields.Length > 0);
        Assert.IsNotNull(result.embed.Footer);
        Assert.IsNotNull(result.embed.Author);
    }
}
