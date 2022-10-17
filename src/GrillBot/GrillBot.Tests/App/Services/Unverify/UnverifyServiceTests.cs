using GrillBot.App.Services.Unverify;
using GrillBot.Common.Managers.Logging;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Services.Unverify;

[TestClass]
public class UnverifyServiceTests : ServiceTest<UnverifyService>
{
    protected override UnverifyService CreateService()
    {
        var discordClient = DiscordHelper.CreateClient();
        var configuration = TestServices.Configuration.Value;
        var texts = new TextsBuilder().Build();
        var checker = new UnverifyChecker(DatabaseBuilder, configuration, TestServices.ProductionEnvironment.Value, texts);
        var profileGenerator = new UnverifyProfileGenerator(DatabaseBuilder, texts);
        var logger = new UnverifyLogger(discordClient, DatabaseBuilder);
        var commandsService = DiscordHelper.CreateCommandsService();
        var interactionService = DiscordHelper.CreateInteractionService(discordClient);
        var loggingManager = new LoggingManager(discordClient, commandsService, interactionService, TestServices.EmptyProvider.Value);
        var messageGenerator = new UnverifyMessageGenerator(texts);

        return new UnverifyService(discordClient, checker, profileGenerator, logger, DatabaseBuilder, loggingManager, texts, messageGenerator, null);
    }

    [TestMethod]
    public async Task GetPendingUnverifiesForRemoveAsync()
    {
        var result = await Service.GetPendingUnverifiesForRemoveAsync();
        Assert.AreEqual(0, result.Count);
    }
}
