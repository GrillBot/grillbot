using System.Linq;
using Discord;
using GrillBot.App.Actions.Commands;
using GrillBot.Data.Enums;
using GrillBot.Data.Models.Guilds;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands;

[TestClass]
public class PermissionsReaderTests : CommandActionTest<PermissionsReader>
{
    private static readonly Overwrite[] Permissions =
    {
        new(Consts.UserId, PermissionTarget.User, new OverwritePermissions(viewChannel: PermValue.Allow)),
        new(Consts.UserId + 1, PermissionTarget.User, new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Allow)),
        new(Consts.UserId + 3, PermissionTarget.User, new OverwritePermissions(ulong.MaxValue, ulong.MaxValue)),
        new(Consts.UserId + 4, PermissionTarget.User, new OverwritePermissions(0, 0)),
        new(Consts.RoleId, PermissionTarget.Role, new OverwritePermissions(ulong.MaxValue, ulong.MaxValue))
    };

    private static readonly ITextChannel TextChannel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetPermissions(Permissions).Build();

    private static readonly IRole Role = new RoleBuilder(Consts.RoleId, Consts.RoleName).Build();
    private static readonly IGuild EmptyGuild = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetRoleAction(Role).Build();

    private static readonly IGuildUser[] Users =
    {
        new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetRoles(new[] { Role.Id }).SetGuild(EmptyGuild).Build(),
        new GuildUserBuilder(Consts.UserId + 1, Consts.Username, Consts.Discriminator).SetRoles(new[] { Role.Id }).SetGuild(EmptyGuild).Build(),
        new GuildUserBuilder(Consts.UserId + 3, Consts.Username, Consts.Discriminator).SetGuildPermissions(GuildPermissions.All).SetGuild(EmptyGuild).Build(),
        new GuildUserBuilder(Consts.UserId + 4, Consts.Username, Consts.Discriminator).SetRoles(new[] { Role.Id }).SetGuild(EmptyGuild).Build(),
        new GuildUserBuilder(Consts.UserId + 5, Consts.Username, Consts.Discriminator).SetRoles(Enumerable.Empty<ulong>()).SetGuild(EmptyGuild).Build(),
        new GuildUserBuilder(Consts.UserId + 6, Consts.Username, Consts.Discriminator).Build()
    };

    private static readonly IGuild MyGuild = new GuildBuilder(Consts.GuildId, Consts.GuildName)
        .SetGetUsersAction(Users)
        .SetGetTextChannelsAction(new[] { TextChannel })
        .Build();

    protected override IMessageChannel Channel => TextChannel;
    protected override IGuild Guild => MyGuild;
    protected override IGuildUser User => Users[0];

    protected override PermissionsReader CreateInstance()
    {
        return InitAction(new PermissionsReader(DatabaseBuilder, TestServices.Texts.Value));
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.User.FromDiscord(Users[4]));
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(MyGuild));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(MyGuild, Users[4]));
        await Repository.AddAsync(new Database.Entity.Unverify
        {
            Reason = "Reason",
            EndAt = DateTime.MaxValue,
            GuildId = Consts.GuildId.ToString(),
            StartAt = DateTime.Today,
            UserId = Users[4].Id.ToString()
        });

        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ReadUselessPermissionsAsync()
    {
        await InitDataAsync();

        var result = await Instance.ReadUselessPermissionsAsync();
        Assert.AreEqual(4, result.Count);
    }

    [TestMethod]
    public void CreateSummary()
    {
        var item = new UselessPermission(TextChannel, User, UselessPermissionType.Administrator);
        var summary = Instance.CreateSummary(new List<UselessPermission> { item });

        Assert.IsFalse(string.IsNullOrEmpty(summary));
        Assert.AreEqual(3, summary.Count(o => o == '1'));
    }
}
