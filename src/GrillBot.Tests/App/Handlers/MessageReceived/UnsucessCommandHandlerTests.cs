using GrillBot.App.Handlers.MessageReceived;
using GrillBot.App.Helpers;
using GrillBot.Cache.Services.Managers;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Handlers.MessageReceived;

[TestClass]
public class UnsucessCommandHandlerTests : TestBase<UnsucessCommandHandler>
{
    protected override UnsucessCommandHandler CreateInstance()
    {
        var interactionService = DiscordHelper.CreateInteractionService(TestServices.DiscordSocketClient.Value);
        var dataCacheManager = new DataCacheManager(CacheBuilder);
        var client = new ClientBuilder().Build();
        var channelHelper = new ChannelHelper(DatabaseBuilder, client);

        return new UnsucessCommandHandler(TestServices.Texts.Value, interactionService, DatabaseBuilder, dataCacheManager, channelHelper, TestServices.RubbergodServiceClient.Value);
    }

    [TestMethod]
    public async Task ProcessAsync_BotMessage()
    {
        var message = new UserMessageBuilder(Consts.MessageId)
            .SetAuthor(new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).AsBot().Build())
            .Build();

        await Instance.ProcessAsync(message);
    }

    [TestMethod]
    public async Task ProcessAsync_DmChannel()
    {
        var message = new UserMessageBuilder(Consts.MessageId)
            .SetAuthor(new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build())
            .SetChannel(new DmChannelBuilder().Build())
            .Build();

        await Instance.ProcessAsync(message);
    }

    [TestMethod]
    public async Task ProcessAsync_NotSlashCommand()
    {
        var message = new UserMessageBuilder(Consts.MessageId)
            .SetAuthor(new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build())
            .SetChannel(new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).Build())
            .SetContent("Test")
            .Build();

        await Instance.ProcessAsync(message);
    }
}
