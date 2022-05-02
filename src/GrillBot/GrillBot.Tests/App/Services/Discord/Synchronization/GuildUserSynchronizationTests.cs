using Discord;
using GrillBot.App.Services.Discord.Synchronization;
using GrillBot.Database.Entity;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;
using Moq;

namespace GrillBot.Tests.App.Services.Discord.Synchronization;

[TestClass]
public class GuildUserSynchronizationTests : ServiceTest<GuildUserSynchronization>
{
    protected override GuildUserSynchronization CreateService()
    {
        return new GuildUserSynchronization(DbFactory);
    }

    [TestMethod]
    public async Task GuildMemberUpdatedAsync_UserNotFound()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var user = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();

        await Service.GuildMemberUpdatedAsync(null, user);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task GuildMemberUpdatedAsync_Ok()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var user = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();

        await DbContext.Users.AddAsync(Database.Entity.User.FromDiscord(user));
        await DbContext.Guilds.AddAsync(Guild.FromDiscord(user.Guild));
        await DbContext.GuildUsers.AddAsync(GuildUser.FromDiscord(user.Guild, user));
        await DbContext.SaveChangesAsync();

        await Service.GuildMemberUpdatedAsync(null, user);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task GuildMemberUpdatedAsync_Bot()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var user = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).AsBot().Build();

        await DbContext.Users.AddAsync(Database.Entity.User.FromDiscord(user));
        await DbContext.Guilds.AddAsync(Guild.FromDiscord(user.Guild));
        await DbContext.GuildUsers.AddAsync(GuildUser.FromDiscord(user.Guild, user));
        await DbContext.SaveChangesAsync();

        await Service.GuildMemberUpdatedAsync(null, user);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task InitUsersAsync()
    {
        var user = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator);
        var botUserBuilder = new GuildUserBuilder().SetIdentity(Consts.UserId + 1, Consts.Username, Consts.Discriminator).AsBot();
        var thirdUser = new GuildUserBuilder().SetIdentity(Consts.UserId + 2, Consts.Username, Consts.Discriminator);

        var guild = new GuildBuilder()
            .SetIdentity(Consts.GuildId, Consts.GuildName)
            .SetGetUsersAction(new[] { user.Build(), botUserBuilder.Build(), thirdUser.Build() })
            .Build();

        var guildUser = user.SetGuild(guild).Build();
        var botUser = botUserBuilder.SetGuild(guild).Build();

        await DbContext.Guilds.AddAsync(Guild.FromDiscord(guild));
        await DbContext.Users.AddAsync(Database.Entity.User.FromDiscord(guildUser));
        await DbContext.GuildUsers.AddAsync(GuildUser.FromDiscord(guild, guildUser));
        await DbContext.Users.AddAsync(Database.Entity.User.FromDiscord(botUser));
        await DbContext.GuildUsers.AddAsync(GuildUser.FromDiscord(guild, botUser));
        await DbContext.SaveChangesAsync();

        await Service.InitUsersAsync(DbContext, guild);
        Assert.IsTrue(true);
    }
}
