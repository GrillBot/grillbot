using GrillBot.App.Services.Discord.Synchronization;
using GrillBot.Database.Entity;
using GrillBot.Tests.Common;
using Org.BouncyCastle.Asn1.Crmf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Services.Discord.Synchronization;

[TestClass]
public class ChannelSynchronizationTests : ServiceTest<ChannelSynchronization>
{
    protected override ChannelSynchronization CreateService()
    {
        return new ChannelSynchronization(DbFactory);
    }

    [TestMethod]
    public async Task ChannelDeletedAsync_ChannelNotFound()
    {
        var channel = DataHelper.CreateTextChannel();

        await Service.ChannelDeletedAsync(channel);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task ChannelDeletedAsync_Ok_WithoutThreads()
    {
        var guild = DataHelper.CreateGuild();
        var channel = DataHelper.CreateTextChannel(mock => mock.Setup(o => o.GuildId).Returns(guild.Id));

        await DbContext.Guilds.AddAsync(Guild.FromDiscord(guild));
        await DbContext.Channels.AddAsync(GuildChannel.FromDiscord(guild, channel, global::Discord.ChannelType.Text));
        await DbContext.SaveChangesAsync();

        await Service.ChannelDeletedAsync(channel);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task ChannelDeletedAsync_Ok()
    {
        var guild = DataHelper.CreateGuild();
        var channel = DataHelper.CreateTextChannel(mock => mock.Setup(o => o.GuildId).Returns(guild.Id));
        var thread = DataHelper.CreateThread();

        await DbContext.Guilds.AddAsync(Guild.FromDiscord(guild));
        await DbContext.Channels.AddAsync(GuildChannel.FromDiscord(guild, channel, global::Discord.ChannelType.Text));
        await DbContext.Channels.AddAsync(GuildChannel.FromDiscord(guild, thread, global::Discord.ChannelType.PrivateThread));
        await DbContext.SaveChangesAsync();

        await Service.ChannelDeletedAsync(channel);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task ThreadDeletedAsync_NotFound()
    {
        var thread = DataHelper.CreateThread();

        await Service.ThreadDeletedAsync(thread);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task ThreadDeletedAsync_Ok()
    {
        var guild = DataHelper.CreateGuild();
        var channel = DataHelper.CreateTextChannel(mock => mock.Setup(o => o.GuildId).Returns(guild.Id));
        var thread = DataHelper.CreateThread(mock => mock.Setup(o => o.GuildId).Returns(guild.Id));

        await DbContext.Guilds.AddAsync(Guild.FromDiscord(guild));
        await DbContext.Channels.AddAsync(GuildChannel.FromDiscord(guild, channel, global::Discord.ChannelType.Text));
        await DbContext.Channels.AddAsync(GuildChannel.FromDiscord(guild, thread, global::Discord.ChannelType.PrivateThread));
        await DbContext.SaveChangesAsync();

        await Service.ThreadDeletedAsync(thread);
        Assert.IsTrue(true);
    }
}
