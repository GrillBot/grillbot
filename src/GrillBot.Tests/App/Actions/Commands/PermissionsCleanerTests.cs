using System.Linq;
using System.Text.RegularExpressions;
using Discord;
using GrillBot.App.Actions.Commands;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands;

[TestClass]
public partial class PermissionsCleanerTests : CommandActionTest<PermissionsCleaner>
{
    private static readonly Overwrite[] Overwrites =
        Enumerable.Range(0, 50).Select(o => new Overwrite(Consts.RoleId + (ulong)o, PermissionTarget.Role, new OverwritePermissions(ulong.MaxValue, 0)))
            .Concat(Enumerable.Range(0, 50).Select(o => new Overwrite(Consts.UserId + (ulong)o, PermissionTarget.User, new OverwritePermissions(ulong.MaxValue, 0))))
            .ToArray();

    private static readonly ITextChannel TextChannel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName)
        .SetGuild(new GuildBuilder(Consts.GuildId, Consts.GuildName).Build())
        .SetPermissions(Overwrites)
        .Build();

    private static readonly IGuildUser[] Users =
        Enumerable.Range(0, 50).Select(o => new GuildUserBuilder(Consts.UserId + (ulong)o, Consts.Username, Consts.Discriminator).SetGuildPermissions(GuildPermissions.All).Build()).ToArray();

    protected override IGuildUser User => Users[0];
    protected override IMessageChannel Channel => TextChannel;

    protected override IGuild Guild { get; } =
        new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetUsersAction(Users).SetGetTextChannelsAction(new[] { TextChannel }).Build();

    protected override PermissionsCleaner CreateInstance()
    {
        var permissionsReader = new PermissionsReader(DatabaseBuilder, TestServices.Texts.Value);
        permissionsReader.Init(Context);

        return InitAction(new PermissionsCleaner(permissionsReader));
    }

    [TestMethod]
    public async Task ClearAllPermissionsAsync()
    {
        Instance.OnProgress = progressBar =>
        {
            Assert.IsFalse(string.IsNullOrEmpty(progressBar));
            Assert.IsTrue(ProgressBarRegex().IsMatch(progressBar));
            return Task.CompletedTask;
        };

        var excludedUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        await Instance.ClearAllPermissionsAsync(TextChannel, new[] { excludedUser });
    }

    [TestMethod]
    public async Task RemoveUselessPermissionsAsync()
    {
        Instance.OnProgress = progressBar =>
        {
            Assert.IsFalse(string.IsNullOrEmpty(progressBar));
            Assert.IsTrue(ProgressBarRegex().IsMatch(progressBar));
            return Task.CompletedTask;
        };

        await Instance.RemoveUselessPermissionsAsync();
    }

    [GeneratedRegex("[▓|░]+ \\(\\d+ %\\) \\*\\*\\d+\\*\\* \\/ \\*\\*\\d+\\*\\*")]
    private static partial Regex ProgressBarRegex();
}
