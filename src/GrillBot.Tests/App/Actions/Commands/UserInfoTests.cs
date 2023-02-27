using System.Linq;
using Discord;
using GrillBot.App.Actions.Commands;
using GrillBot.Data.Models.Unverify;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;
using Newtonsoft.Json;

namespace GrillBot.Tests.App.Actions.Commands;

[TestClass]
public class UserInfoTests : CommandActionTest<UserInfo>
{
    private static readonly IRole RoleWithColor = new RoleBuilder(Consts.RoleId, Consts.RoleName).Build();
    private static readonly IGuild EmptyGuild = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetRoleAction(RoleWithColor).Build();

    private static readonly IGuildUser GuildUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(EmptyGuild)
        .SetRoles(Enumerable.Repeat(RoleWithColor.Id, 50)).SetActiveDevices(new[] { ClientType.Desktop }).SetPremiumSinceDate(DateTimeOffset.Now).Build();

    protected override IGuild Guild { get; }
        = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetUsersAction(new[] { GuildUser }).Build();

    protected override IMessageChannel Channel { get; }
        = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(EmptyGuild).Build();

    protected override IGuildUser User => GuildUser;

    protected override UserInfo CreateInstance()
    {
        return InitAction(new UserInfo(DatabaseBuilder, TestServices.Configuration.Value, TestServices.Texts.Value));
    }

    private async Task InitDataAsync()
    {
        var user = Database.Entity.User.FromDiscord(GuildUser);
        user.Flags |= (int)(UserFlags.WebAdmin | UserFlags.BotAdmin);

        var guildUser = Database.Entity.GuildUser.FromDiscord(Guild, GuildUser);
        guildUser.ObtainedReactions = 500;
        guildUser.GivenReactions = 500;
        guildUser.UsedInvite = new Invite
        {
            Code = Consts.InviteCode,
            CreatedAt = DateTime.Now,
            CreatorId = Consts.UserId.ToString(),
            GuildId = Consts.GuildId.ToString()
        };

        guildUser.Unverify = new Database.Entity.Unverify
        {
            Reason = "Reason",
            Channels = new List<GuildChannelOverride>(),
            Roles = new List<string>(),
            EndAt = DateTime.MaxValue,
            GuildId = Consts.GuildId.ToString(),
            StartAt = DateTime.MinValue,
            UnverifyLog = new UnverifyLog
            {
                Data = JsonConvert.SerializeObject(new UnverifyLogSet()),
                Operation = UnverifyOperation.Unverify,
                CreatedAt = DateTime.Now,
                GuildId = Consts.GuildId.ToString(),
                FromUserId = Consts.UserId.ToString(),
                ToUserId = Consts.UserId.ToString(),
                Id = 1
            },
            UserId = Consts.UserId.ToString()
        };

        await Repository.AddAsync(new UnverifyLog
        {
            Data = JsonConvert.SerializeObject(new UnverifyLogSet { IsSelfUnverify = true }),
            Operation = UnverifyOperation.Selfunverify,
            CreatedAt = DateTime.Now,
            GuildId = Consts.GuildId.ToString(),
            FromUserId = Consts.UserId.ToString(),
            ToUserId = Consts.UserId.ToString(),
            Id = 2
        });

        await Repository.AddAsync(new PointsTransaction
        {
            AssingnedAt = DateTime.Today,
            GuildId = Consts.GuildId.ToString(),
            Points = 100,
            UserId = Consts.UserId.ToString(),
            MessageId = SnowflakeUtils.ToSnowflake(DateTimeOffset.Now).ToString(),
            ReactionId = ""
        });
        await Repository.AddAsync(new GuildUserChannel
        {
            Count = 50,
            ChannelId = Consts.ChannelId.ToString(),
            GuildId = Consts.GuildId.ToString(),
            UserId = Consts.UserId.ToString(),
            FirstMessageAt = DateTime.MinValue,
            LastMessageAt = DateTime.MaxValue
        });
        await Repository.AddAsync(GuildChannel.FromDiscord((ITextChannel)Channel, ChannelType.Text));
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.AddAsync(user);
        await Repository.AddAsync(guildUser);
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync()
    {
        await InitDataAsync();
        var result = await Instance.ProcessAsync(GuildUser);

        Assert.IsNotNull(result?.Fields);
        Assert.IsTrue(result.Fields.Length > 0);
        Assert.IsNotNull(result.Author);
        Assert.IsNotNull(result.Footer);
    }
}
