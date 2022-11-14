using Discord;
using GrillBot.App.Services.Unverify;
using GrillBot.Data.Models;
using GrillBot.Data.Models.Unverify;
using GrillBot.Tests.Infrastructure.Discord;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Services.Unverify;

[TestClass]
public class UnverifyLoggerTests : ServiceTest<UnverifyLogger>
{
    protected override UnverifyLogger CreateService()
    {
        var discordClient = DiscordHelper.CreateClient();
        return new UnverifyLogger(discordClient, DatabaseBuilder);
    }

    [TestMethod]
    public async Task LogUnverifyAsync()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var fromUser = new GuildUserBuilder(Consts.UserId + 1, Consts.Username + "2", Consts.Discriminator).SetGuild(guild).Build();

        var profile = new UnverifyUserProfile(guildUser, DateTime.MinValue, DateTime.MaxValue, false, "cs");
        var logItem = await Service.LogUnverifyAsync(profile, guild, fromUser);

        Assert.IsNotNull(logItem);
        Assert.IsTrue(logItem.Id > 0);
    }

    [TestMethod]
    public async Task LogSelfUnverifyAsync()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var profile = new UnverifyUserProfile(guildUser, DateTime.MinValue, DateTime.MaxValue, false, "cs");
        var logItem = await Service.LogSelfunverifyAsync(profile, guild);

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

        await Service.LogRemoveAsync(returnedRoles, returnedChannels, guild, fromUser, toUser, false, "cs");
    }

    [TestMethod]
    public async Task LogUpdateAsync()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var toUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var fromUser = new GuildUserBuilder(Consts.UserId + 1, Consts.Username + "2", Consts.Discriminator).SetGuild(guild).Build();

        await Service.LogUpdateAsync(DateTime.MinValue, DateTime.MaxValue, guild, fromUser, toUser);
    }

    [TestMethod]
    public async Task LogRecoverAsync()
    {
        var returnedRoles = new List<IRole> { new RoleBuilder(Consts.RoleId, Consts.RoleName).Build() };
        var returnedChannels = new List<ChannelOverride>();
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var toUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var fromUser = new GuildUserBuilder(Consts.UserId + 1, Consts.Username + "2", Consts.Discriminator).SetGuild(guild).Build();

        await Service.LogRecoverAsync(returnedRoles, returnedChannels, guild, fromUser, toUser);
    }
}
