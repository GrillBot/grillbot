using Discord;
using GrillBot.App.Managers;
using GrillBot.Data.Models;
using GrillBot.Data.Models.Unverify;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Managers;

[TestClass]
public class UnverifyLogManagerTests : TestBase<UnverifyLogManager>
{
    protected override UnverifyLogManager CreateInstance()
    {
        return new UnverifyLogManager(TestServices.DiscordSocketClient.Value, DatabaseBuilder);
    }

    [TestMethod]
    public async Task LogUnverifyAsync()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var fromUser = new GuildUserBuilder(Consts.UserId + 1, Consts.Username + "2", Consts.Discriminator).SetGuild(guild).Build();

        var profile = new UnverifyUserProfile(guildUser, DateTime.MinValue, DateTime.MaxValue, false, "cs");
        var logItem = await Instance.LogUnverifyAsync(profile, guild, fromUser);

        Assert.IsNotNull(logItem);
        Assert.IsTrue(logItem.Id > 0);
    }

    [TestMethod]
    public async Task LogSelfUnverifyAsync()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var profile = new UnverifyUserProfile(guildUser, DateTime.MinValue, DateTime.MaxValue, false, "cs");
        var logItem = await Instance.LogSelfunverifyAsync(profile, guild);

        Assert.IsNotNull(logItem);
        Assert.IsTrue(logItem.Id > 0);
    }

    [TestMethod]
    public async Task LogRemoveAsync()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();

        var toUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var fromUser = new GuildUserBuilder(Consts.UserId + 1, Consts.Username + "2", Consts.Discriminator).SetGuild(guild).Build();
        var returnedRoles = new List<IRole> { new RoleBuilder(Consts.RoleId, Consts.RoleName).Build() };
        var returnedChannels = new List<ChannelOverride>();

        await Instance.LogRemoveAsync(returnedRoles, returnedChannels, guild, fromUser, toUser, false, false, "cs");
    }

    [TestMethod]
    public async Task LogUpdateAsync()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var toUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var fromUser = new GuildUserBuilder(Consts.UserId + 1, Consts.Username + "2", Consts.Discriminator).SetGuild(guild).Build();

        await Instance.LogUpdateAsync(DateTime.MinValue, DateTime.MaxValue, guild, fromUser, toUser, "Reason");
    }

    [TestMethod]
    public async Task LogRecoverAsync()
    {
        var returnedRoles = new List<IRole> { new RoleBuilder(Consts.RoleId, Consts.RoleName).Build() };
        var returnedChannels = new List<ChannelOverride>();
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var toUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var fromUser = new GuildUserBuilder(Consts.UserId + 1, Consts.Username + "2", Consts.Discriminator).SetGuild(guild).Build();

        await Instance.LogRecoverAsync(returnedRoles, returnedChannels, guild, fromUser, toUser);
    }
}
