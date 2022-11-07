using System.Diagnostics.CodeAnalysis;
using System.IO;
using Discord;
using GrillBot.App.Actions.Api.V1.Channel;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Channel;

[TestClass]
public class SendMessageToChannelTests : ApiActionTest<SendMessageToChannel>
{
    protected override SendMessageToChannel CreateAction()
    {
        var guildBuilder = new GuildBuilder(Consts.GuildId, Consts.GuildName);
        var message = new UserMessageBuilder(Consts.MessageId).Build();
        var textChannel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetSendFilesAction(message).SetSendMessageAction(message).Build();
        guildBuilder.SetGetTextChannelAction(textChannel);

        var texts = new TextsBuilder()
            .AddText("ChannelModule/PostMessage/GuildNotFound", "cs", "GuildNotFound")
            .AddText("ChannelModule/PostMessage/ChannelNotFound", "cs", "ChannelNotFound")
            .Build();
        var client = new ClientBuilder()
            .SetGetGuildAction(guildBuilder.Build())
            .Build();
        var messageCache = new MessageCacheBuilder()
            .SetGetAction(Consts.MessageId, message)
            .Build();

        return new SendMessageToChannel(ApiRequestContext, texts, client, messageCache);
    }

    protected override void Cleanup()
    {
        if (File.Exists("SendMessageToChannel.txt"))
            File.Delete("SendMessageToChannel.txt");
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_GuildNotFound()
        => await Action.ProcessAsync(Consts.GuildId + 1, Consts.ChannelId, new SendMessageToChannelParams());

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_ChannelNotFound()
        => await Action.ProcessAsync(Consts.GuildId, Consts.ChannelId + 1, new SendMessageToChannelParams());

    [TestMethod]
    public async Task ProcessAsync_Success_WithoutReference()
        => await Action.ProcessAsync(Consts.GuildId, Consts.ChannelId, new SendMessageToChannelParams { Content = "Zprava" });

    [TestMethod]
    public async Task ProcessAsync_Success_WithIdReference()
        => await Action.ProcessAsync(Consts.GuildId, Consts.ChannelId, new SendMessageToChannelParams { Content = "Zprava", Reference = (Consts.MessageId + 1).ToString() });

    [TestMethod]
    public async Task ProcessAsync_Success_NotUri()
        => await Action.ProcessAsync(Consts.GuildId, Consts.ChannelId, new SendMessageToChannelParams { Content = "Zprava", Reference = "ReferenceLink" });

    [TestMethod]
    public async Task ProcessAsync_Success_InvalidUri()
        => await Action.ProcessAsync(Consts.GuildId, Consts.ChannelId, new SendMessageToChannelParams { Content = "Zprava", Reference = "https://grillbot.cloud" });

    [TestMethod]
    public async Task ProcessAsync_Success_ValidUri()
    {
        await Action.ProcessAsync(Consts.GuildId, Consts.ChannelId,
            new SendMessageToChannelParams { Content = "Zprava", Reference = $"https://discord.com/channels/{Consts.GuildId}/{Consts.ChannelId}/{Consts.MessageId}" });
    }

    [TestMethod]
    public async Task ProcessAsync_Success_WithAttachments()
    {
        await File.WriteAllBytesAsync("SendMessageToChannel.txt", new byte[] { 1, 2, 3, 4, 5 });

        using var attachment = new FileAttachment("SendMessageToChannel.txt");
        var parameters = new SendMessageToChannelParams { Content = "Zprava" };
        parameters.Attachments.Add(attachment);

        await Action.ProcessAsync(Consts.GuildId, Consts.ChannelId, parameters);
    }
}
