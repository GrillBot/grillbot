﻿using System.Diagnostics.CodeAnalysis;
using Discord;
using GrillBot.App.Actions.Api.V1.Channel;
using GrillBot.Data.Exceptions;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Channel;

[TestClass]
public class GetChannelDetailTests : ApiActionTest<GetChannelDetail>
{
    private IGuild Guild { get; set; }
    private ITextChannel TextChannel { get; set; }

    protected override GetChannelDetail CreateAction()
    {
        var guildBuilder = new GuildBuilder(Consts.GuildId, Consts.GuildName);
        TextChannel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(guildBuilder.Build()).Build();
        Guild = guildBuilder.SetGetTextChannelsAction(new[] { TextChannel }).Build();

        var client = new ClientBuilder().SetGetGuildsAction(new[] { Guild }).Build();
        var messageCache = new MessageCacheBuilder().Build();

        return new GetChannelDetail(ApiRequestContext, DatabaseBuilder, TestServices.Texts.Value, TestServices.AutoMapper.Value, client, messageCache);
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_ChannelNotFound()
        => await Action.ProcessAsync(Consts.ChannelId + 1);

    [TestMethod]
    public async Task ProcessAsync_DeletedChannel()
    {
        await InitChannelAsync((long)ChannelFlag.Deleted);

        var result = await Action.ProcessAsync(Consts.ChannelId);
        Assert.IsNotNull(result);
        Assert.AreEqual((long)ChannelFlag.Deleted, result.Flags);
    }

    [TestMethod]
    public async Task ProcessAsync_GuildChannelNotFound()
    {
        await InitChannelAsync(0, Consts.ChannelId + 1);

        var result = await Action.ProcessAsync(Consts.ChannelId + 1);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        await InitChannelAsync(0);

        var result = await Action.ProcessAsync(Consts.ChannelId);
        Assert.IsNotNull(result);
    }

    private async Task InitChannelAsync(long flags, ulong id = 0)
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));

        var channel = Database.Entity.GuildChannel.FromDiscord(TextChannel, ChannelType.Text);
        if (id > 0) channel.ChannelId = id.ToString();
        channel.Flags = flags;
        await Repository.AddAsync(channel);
        await Repository.CommitAsync();
    }
}
