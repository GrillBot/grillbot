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
        return new GuildUserSynchronization(DatabaseBuilder);
    }

    [TestMethod]
    public async Task GuildMemberUpdatedAsync_UserNotFound()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var user = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();

        await Service.GuildMemberUpdatedAsync(user);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task GuildMemberUpdatedAsync_Ok()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var user = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();

        await Repository.AddAsync(Database.Entity.User.FromDiscord(user));
        await Repository.AddAsync(Guild.FromDiscord(user.Guild));
        await Repository.AddAsync(GuildUser.FromDiscord(user.Guild, user));
        await Repository.CommitAsync();

        await Service.GuildMemberUpdatedAsync(user);
    }

    [TestMethod]
    public async Task GuildMemberUpdatedAsync_Bot()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var user = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).AsBot().Build();

        await Repository.AddAsync(Database.Entity.User.FromDiscord(user));
        await Repository.AddAsync(Guild.FromDiscord(user.Guild));
        await Repository.AddAsync(GuildUser.FromDiscord(user.Guild, user));
        await Repository.CommitAsync();

        await Service.GuildMemberUpdatedAsync(user);
        Assert.IsTrue(true);
    }
}
