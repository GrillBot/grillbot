using GrillBot.App.Services;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Logging;
using GrillBot.App.Services.User.Points;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Managers;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Services.User.Points;

[TestClass]
public class PointsJobTests : JobTest<PointsJob>
{
    protected override PointsJob CreateJob()
    {
        var discordClient = DiscordHelper.CreateClient();
        var configuration = TestServices.Configuration.Value;
        var loggingFactory = LoggingHelper.CreateLoggerFactory();
        var initManager = new InitManager(loggingFactory);
        var counter = TestServices.CounterManager.Value;
        var messageCache = new MessageCacheManager(discordClient, initManager, CacheBuilder, counter);
        var randomization = new RandomizationService();
        var profilePictures = new ProfilePictureManager(CacheBuilder, counter);
        var pointsService = new PointsService(discordClient, DatabaseBuilder, configuration, messageCache, randomization, profilePictures);
        var client = new ClientBuilder().Build();
        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);
        var commandsService = DiscordHelper.CreateCommandsService();
        var interactionService = DiscordHelper.CreateInteractionService(discordClient);
        var loggingService = new LoggingService(discordClient, commandsService, loggingFactory, configuration,
            DatabaseBuilder, interactionService);

        return new PointsJob(loggingService, auditLogWriter, client, initManager, pointsService);
    }

    [TestMethod]
    public async Task RunAsync()
    {
        var context = CreateContext();
        await Job.Execute(context);

        Assert.IsNotNull(context.Result);
    }
}
