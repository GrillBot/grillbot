using Discord;
using GrillBot.App.Services.Discord.Synchronization;
using GrillBot.Database.Entity;
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
        var user = DataHelper.CreateGuildUser();

        await Service.GuildMemberUpdatedAsync(null, user);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task GuildMemberUpdatedAsync_Ok()
    {
        var user = DataHelper.CreateGuildUser();

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
        var user = DataHelper.CreateGuildUser(bot: true);

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
        var guildUser = DataHelper.CreateGuildUser();
        var botUser = DataHelper.CreateGuildUser(id: 968525658, bot: true);

        var guild = DataHelper.CreateGuild(mock =>
        {
            mock.Setup(o => o.GetUsersAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(new List<IGuildUser>()
            {
                guildUser, botUser,
                DataHelper.CreateGuildUser(id: 9685658)
            }.AsReadOnly() as IReadOnlyCollection<IGuildUser>));
        });

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
