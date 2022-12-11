using Discord;
using GrillBot.App.Services.Discord.Synchronization;
using GrillBot.Database.Entity;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Services.Discord.Synchronization;

[TestClass]
public class ChannelSynchronizationTests : ServiceTest<ChannelSynchronization>
{
    protected override ChannelSynchronization CreateService()
    {
        return new ChannelSynchronization(DatabaseBuilder);
    }

    [TestMethod]
    public async Task ChannelDeletedAsync_ChannelNotFound()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var channel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(guild).Build();

        await Service.ChannelDeletedAsync(channel);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task ChannelDeletedAsync_Ok_WithoutThreads()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var channel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(guild).Build();

        await Repository.AddAsync(Guild.FromDiscord(guild));
        await Repository.AddAsync(GuildChannel.FromDiscord(channel, ChannelType.Text));
        await Repository.CommitAsync();

        await Service.ChannelDeletedAsync(channel);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task ChannelDeletedAsync_Ok()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var channel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(guild).Build();
        var thread = new ThreadBuilder(Consts.ThreadId, Consts.ThreadName).SetGuild(guild).SetType(ThreadType.PrivateThread).Build();

        await Repository.AddAsync(Guild.FromDiscord(guild));
        await Repository.AddAsync(GuildChannel.FromDiscord(channel, ChannelType.Text));
        await Repository.AddAsync(GuildChannel.FromDiscord(thread, ChannelType.PrivateThread));
        await Repository.CommitAsync();

        await Service.ChannelDeletedAsync(channel);
        Assert.IsTrue(true);
    }
}
