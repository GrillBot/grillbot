﻿using Discord;
using Discord.Rest;
using GrillBot.App.Handlers.ChannelUpdated;
using GrillBot.App.Services.AuditLog;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Handlers.ChannelUpdated;

[TestClass]
public class AuditChannelUpdatedHandlerTests : HandlerTest<AuditChannelUpdatedHandler>
{
    private ITextChannel TextChannel { get; set; }

    protected override AuditChannelUpdatedHandler CreateHandler()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        TextChannel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(guild).Build();

        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);
        return new AuditChannelUpdatedHandler(TestServices.CounterManager.Value, auditLogWriter);
    }

    [TestMethod]
    public async Task ProcessAsync_Dms()
    {
        var dms = new DmChannelBuilder().Build();
        await Handler.ProcessAsync(dms, dms);
    }

    [TestMethod]
    public async Task ProcessAsync_Equals()
    {
        await Handler.ProcessAsync(TextChannel, TextChannel);
    }

    [TestMethod]
    public async Task ProcessAsync_NoAuditLog()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetAuditLogsAction(new List<IAuditLogEntry>()).Build();
        var channel = new TextChannelBuilder(Consts.ChannelId + 1, Consts.ChannelName).SetGuild(guild).Build();

        await Handler.ProcessAsync(TextChannel, channel);
    }

    [TestMethod]
    public async Task ProcessAsync_PositionChange()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetAuditLogsAction(new List<IAuditLogEntry>()).Build();
        var channel = new TextChannelBuilder(Consts.ChannelId + 1, Consts.ChannelName).SetGuild(guild).SetPosition(50).Build();

        await Handler.ProcessAsync(TextChannel, channel);
    }

    [TestMethod]
    public async Task ProcessAsync_WithAuditLog()
    {
        var info = ReflectionHelper.CreateWithInternalConstructor<ChannelInfo>("", "", null, null, null, null);
        var data = ReflectionHelper.CreateWithInternalConstructor<ChannelUpdateAuditLogData>(Consts.ChannelId, info, info);
        var user = new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var entry = new AuditLogEntryBuilder(Consts.AuditLogEntryId).SetUser(user).SetData(data).Build();
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetAuditLogsAction(new List<IAuditLogEntry> { entry }).Build();
        var channel = new TextChannelBuilder(Consts.ChannelId + 1, Consts.ChannelName).SetGuild(guild).Build();

        await Handler.ProcessAsync(TextChannel, channel);
    }
}
