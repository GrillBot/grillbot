using Discord;
using GrillBot.App.Handlers.GuildMemberUpdated;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Handlers.GuildMemberUpdated;

[TestClass]
public class ServerBoosterHandlerTests : HandlerTest<ServerBoosterHandler>
{
    private IGuildUser Before { get; set; }
    private IGuildUser After { get; set; }
    private IRole Role { get; set; }

    protected override ServerBoosterHandler CreateHandler()
    {
        Role = new RoleBuilder(Consts.RoleId, Consts.RoleName).Build();
        var adminChannel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).Build();
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetTextChannelsAction(new[] { adminChannel }).SetRoles(new[] { Role }).Build();
        Before = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetRoles(new List<ulong>()).SetGuild(guild).Build();
        After = new GuildUserBuilder(Before).SetRoles(new[] { Role.Id }).SetGuild(guild).Build();

        return new ServerBoosterHandler(DatabaseBuilder, TestServices.Configuration.Value);
    }

    private async Task InitDataAsync(ulong? boosterRoleId, ulong? adminChannelId)
    {
        var guild = Database.Entity.Guild.FromDiscord(After.Guild);
        guild.BoosterRoleId = boosterRoleId?.ToString();
        guild.AdminChannelId = adminChannelId?.ToString();
        await Repository.AddAsync(guild);
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_CannotProcess()
    {
        var user = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetRoles(new List<ulong>()).Build();
        await Handler.ProcessAsync(user, user);
    }

    [TestMethod]
    public async Task ProcessAsync_GuildNotFound()
    {
        await Handler.ProcessAsync(Before, After);
    }

    [TestMethod]
    public async Task ProcessAsync_NoBoosterRoleId()
    {
        await InitDataAsync(null, null);
        await Handler.ProcessAsync(Before, After);
    }

    [TestMethod]
    public async Task ProcessAsync_NoAdminChannel()
    {
        await InitDataAsync(Consts.RoleId + 1, null);
        await Handler.ProcessAsync(Before, After);
    }

    [TestMethod]
    public async Task ProcessAsync_Ok_None()
    {
        await InitDataAsync(Consts.RoleId, Consts.ChannelId);

        var before = new GuildUserBuilder(Before).SetRoles(new[] { Consts.RoleId + 1 }).SetGuild(After.Guild).Build();
        var after = new GuildUserBuilder(Before).SetRoles(new[] { Consts.RoleId + 2 }).SetGuild(After.Guild).Build();
        await Handler.ProcessAsync(before, after);
    }

    [TestMethod]
    public async Task ProcessAsync_BoostRoleNotFound()
    {
        await InitDataAsync(Consts.RoleId + 1, Consts.ChannelId);
        await Handler.ProcessAsync(Before, After);
    }

    [TestMethod]
    public async Task ProcessAsync_ChannelNotFound()
    {
        await InitDataAsync(Consts.RoleId, Consts.ChannelId + 1);
        await Handler.ProcessAsync(Before, After);
    }

    [TestMethod]
    public async Task ProcessAsync_Ok_Added()
    {
        await InitDataAsync(Consts.RoleId, Consts.ChannelId);
        await Handler.ProcessAsync(Before, After);
    }

    [TestMethod]
    public async Task ProcessAsync_Ok_Removed()
    {
        await InitDataAsync(Consts.RoleId, Consts.ChannelId);
        await Handler.ProcessAsync(After, Before);
    }
}
