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
    private static readonly IRole RoleWithColor = new RoleBuilder().SetIdentity(Consts.RoleId, Consts.RoleName).Build();
    private static readonly IGuild EmptyGuild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).SetGetRoleAction(RoleWithColor).Build();

    private static readonly IGuildUser GuildUser = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(EmptyGuild).SetRoles(new[] { RoleWithColor })
        .SetActiveDevices(new[] { ClientType.Desktop }).SetPremiumSinceDate(DateTimeOffset.Now).Build();

    protected override IGuild Guild { get; }
        = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).SetGetUsersAction(new[] { GuildUser }).Build();

    protected override IMessageChannel Channel { get; }
        = new TextChannelBuilder().SetIdentity(Consts.ChannelId, Consts.ChannelName).SetGuild(EmptyGuild).Build();

    protected override IGuildUser User => GuildUser;

    protected override UserInfo CreateAction()
    {
        var texts = new TextsBuilder()
            .AddText("User/InfoEmbed/Title", "en-US", "Title")
            .AddText("User/UserStatus/Offline", "en-US", "Offline")
            .AddText("User/InfoEmbed/Fields/State", "en-US", "State")
            .AddText("User/InfoEmbed/Fields/CreatedAt", "en-US", "CreatedAt")
            .AddText("User/InfoEmbed/Fields/ActiveDevices", "en-US", "ActiveDevices")
            .AddText("User/InfoEmbed/Fields/Roles", "en-US", "Roles")
            .AddText("User/InfoEmbed/NoRoles", "en-US", "NoRoles")
            .AddText("User/InfoEmbed/Fields/JoinedAt", "en-US", "JoinedAt")
            .AddText("User/InfoEmbed/Fields/PremiumSince", "en-US", "PremiumSince")
            .AddText("User/InfoEmbed/Fields/Reactions", "en-US", "Reactions")
            .AddText("User/InfoEmbed/Fields/Points", "en-US", "Points")
            .AddText("User/InfoEmbed/Fields/MessageCount", "en-US", "MessageCount")
            .AddText("User/InfoEmbed/Fields/UnverifyCount", "en-US", "UnverifyCount")
            .AddText("User/InfoEmbed/Fields/SelfUnverifyCount", "en-US", "SelfUnverifyCount")
            .AddText("User/InfoEmbed/Fields/UnverifyInfo", "en-US", "UnverifyInfo")
            .AddText("User/InfoEmbed/ReasonRow", "en-US", "ReasonRow")
            .AddText("User/InfoEmbed/UnverifyRow", "en-US", "UnverifyRow")
            .AddText("User/InfoEmbed/UsedVanityInviteRow", "en-US", "UsedVanityInviteRow")
            .AddText("User/InfoEmbed/UsedInviteRow", "en-US", "UsedInviteRow")
            .AddText("User/InfoEmbed/VanityInvite", "en-US", "VanityInvite")
            .AddText("User/InfoEmbed/Fields/UsedInvite", "en-US", "UsedInvite")
            .AddText("User/InfoEmbed/Fields/MostActiveChannel", "en-US", "MostActiveChannel")
            .AddText("User/InfoEmbed/Fields/LastMessageIn", "en-US", "LastMessageIn")
            .Build();

        return InitAction(new UserInfo(DatabaseBuilder, TestServices.Configuration.Value, texts));
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

        await Repository.AddAsync(new PointsTransactionSummary
        {
            Day = DateTime.Today,
            GuildId = Consts.GuildId.ToString(),
            MessagePoints = 50,
            ReactionPoints = 50,
            UserId = Consts.UserId.ToString()
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
        var result = await Action.ProcessAsync(GuildUser);

        Assert.IsNotNull(result?.Fields);
        Assert.IsTrue(result.Fields.Length > 0);
        Assert.IsNotNull(result.Author);
        Assert.IsNotNull(result.Footer);
    }
}
