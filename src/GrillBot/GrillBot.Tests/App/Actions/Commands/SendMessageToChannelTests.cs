using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Discord;
using GrillBot.App.Actions.Commands;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands;

[TestClass]
public class SendMessageToChannelTests : CommandActionTest<SendMessageToChannel>
{
    private static readonly ITextChannel TextChannel = new TextChannelBuilder().SetIdentity(Consts.ChannelId, Consts.ChannelName).SetGuild(new GuildBuilder().SetId(Consts.GuildId).Build()).Build();
    private static readonly IGuild GuildData = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).SetGetChannelsAction(new[] { TextChannel }).Build();

    protected override IMessageChannel Channel => TextChannel;
    protected override IGuild Guild => GuildData;

    protected override SendMessageToChannel CreateAction()
    {
        var httpClientFactory = HttpClientHelper.CreateFactory(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(new byte[] { 1, 2, 3, 4, 5 }) });
        var apiContext = new ApiRequestContext { Language = "en-US", LoggedUser = User };
        var texts = new TextsBuilder().AddText("ChannelModule/PostMessage/NoContent", "en-US", "NoContent").Build();
        var client = new ClientBuilder().SetGetGuildsAction(new[] { Guild }).Build();
        var messageCache = new MessageCacheBuilder().Build();
        var action = new GrillBot.App.Actions.Api.V1.Channel.SendMessageToChannel(apiContext, texts, client, messageCache);
        return InitAction(new SendMessageToChannel(httpClientFactory, action, texts));
    }

    [TestMethod]
    public async Task ProcessAsync_OnlyText()
        => await Action.ProcessAsync((ITextChannel)Channel, null, "Content", new IAttachment[] { null });

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_ValidationFailed()
        => await Action.ProcessAsync((ITextChannel)Channel, null, null, new IAttachment[] { null });

    [TestMethod]
    public async Task ProcessAsync_WithAttachments()
    {
        var attachment = new AttachmentBuilder().SetFilename("File.png").SetUrl("https://grillbot.tests/File.png").Build();
        await Action.ProcessAsync((ITextChannel)Channel, null, null, new[] { attachment });
    }
}
