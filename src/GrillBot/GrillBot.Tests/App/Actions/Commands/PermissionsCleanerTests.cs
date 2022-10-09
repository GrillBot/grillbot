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
        Enumerable.Range(0, 50).Select(o => new Overwrite(Consts.RoleId + (ulong)o, PermissionTarget.Role, OverwritePermissions.InheritAll))
            .Concat(Enumerable.Range(0, 50).Select(o => new Overwrite(Consts.UserId + (ulong)o, PermissionTarget.User, OverwritePermissions.InheritAll)))
            .ToArray();

    private static readonly ITextChannel TextChannel = new TextChannelBuilder()
        .SetIdentity(Consts.ChannelId, Consts.ChannelName)
        .SetGuild(new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build())
        .SetPermissions(Overwrites)
        .Build();

    protected override IMessageChannel Channel => TextChannel;

    protected override PermissionsCleaner CreateAction()
    {
        return InitAction(new PermissionsCleaner());
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
}
