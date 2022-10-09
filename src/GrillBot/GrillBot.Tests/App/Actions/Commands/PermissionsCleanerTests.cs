using System.Linq;
using System.Text.RegularExpressions;
using Discord;
using GrillBot.App.Actions.Commands;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands;

[TestClass]
public class PermissionsCleanerTests : CommandActionTest<PermissionsCleaner>
{
    private static readonly Overwrite[] Overwrites =
        Enumerable.Range(0, 50).Select(o => new Overwrite(Consts.RoleId + (ulong)o, PermissionTarget.Role, new OverwritePermissions(ulong.MaxValue, 0)))
            .Concat(Enumerable.Range(0, 50).Select(o => new Overwrite(Consts.UserId + (ulong)o, PermissionTarget.User, new OverwritePermissions(ulong.MaxValue, 0))))
            .ToArray();

    private static readonly ITextChannel TextChannel = new TextChannelBuilder()
        .SetIdentity(Consts.ChannelId, Consts.ChannelName)
        .SetGuild(new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build())
        .SetPermissions(Overwrites)
        .Build();

    private static readonly IGuildUser[] Users =
        Enumerable.Range(0, 50)
            .Select(o => new GuildUserBuilder().SetIdentity(Consts.UserId + (ulong)o, Consts.Username, Consts.Discriminator).SetGuildPermissions(GuildPermissions.All).Build())
            .ToArray();

    protected override IGuildUser User => Users[0];
    protected override IMessageChannel Channel => TextChannel;

    protected override IGuild Guild { get; } =
        new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).SetGetUsersAction(Users).SetGetChannelsAction(new[] { TextChannel }).Build();

    protected override PermissionsCleaner CreateAction()
    {
        var texts = new TextsBuilder().Build();
        var permissionsReader = new PermissionsReader(DatabaseBuilder, texts);
        permissionsReader.Init(Context);

        return InitAction(new PermissionsCleaner(permissionsReader));
    }

    [TestMethod]
    public async Task ClearAllPermissionsAsync()
    {
        Action.OnProgress = progressBar =>
        {
            Assert.IsFalse(string.IsNullOrEmpty(progressBar));
            Assert.IsTrue(Regex.IsMatch(progressBar, @"[▓|░]+ \(\d+ %\) \*\*\d+\*\* \/ \*\*\d+\*\*"));
            return Task.CompletedTask;
        };

        var excludedUser = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        await Action.ClearAllPermissionsAsync(TextChannel, new[] { excludedUser });
    }

    [TestMethod]
    public async Task RemoveUselessPermissionsAsync()
    {
        Action.OnProgress = progressBar =>
        {
            Assert.IsFalse(string.IsNullOrEmpty(progressBar));
            Assert.IsTrue(Regex.IsMatch(progressBar, @"[▓|░]+ \(\d+ %\) \*\*\d+\*\* \/ \*\*\d+\*\*"));
            return Task.CompletedTask;
        };

        await Action.RemoveUselessPermissionsAsync();
    }
}
