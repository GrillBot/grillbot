using GrillBot.App.Services.Logging;
using GrillBot.App.Services.Unverify;
using GrillBot.Tests.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Services.Unverify;

[TestClass]
public class UnverifyServiceTests : ServiceTest<UnverifyService>
{
    protected override UnverifyService CreateService()
    {
        var discordClient = DiscordHelper.CreateClient();
        var dbFactory = new DbContextBuilder();
        var configuration = ConfigurationHelper.CreateConfiguration();
        var environment = EnvironmentHelper.CreateEnv("Production");
        var checker = new UnverifyChecker(dbFactory, configuration, environment);
        var profileGenerator = new UnverifyProfileGenerator(dbFactory);
        var logger = new UnverifyLogger(discordClient, dbFactory);
        var commandsService = DiscordHelper.CreateCommandsService();
        var loggerFactory = LoggingHelper.CreateLoggerFactory();
        var interactionService = DiscordHelper.CreateInteractionService(discordClient);
        var loggingService = new LoggingService(discordClient, commandsService, loggerFactory, configuration, dbFactory, interactionService);

        return new UnverifyService(discordClient, checker, profileGenerator, logger, dbFactory, loggingService);
    }

    [TestMethod]
    public async Task GetUnverifyCountsOfGuildAsync()
    {
        var guild = DataHelper.CreateGuild();
        var result = await Service.GetUnverifyCountsOfGuildAsync(guild);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public async Task GetUserIdsWithUnverifyAsync()
    {
        var guild = DataHelper.CreateGuild();
        var result = await Service.GetUserIdsWithUnverifyAsync(guild);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetPendingUnverifiesForRemoveAsync()
    {
        var result = await Service.GetPendingUnverifiesForRemoveAsync(CancellationToken.None);
        Assert.AreEqual(0, result.Count);
    }
}
