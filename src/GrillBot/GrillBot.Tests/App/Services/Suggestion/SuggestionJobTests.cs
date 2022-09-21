using System.Linq;
using Discord;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Suggestion;
using GrillBot.Cache.Services.Managers;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Logging;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Services.Suggestion;

[TestClass]
public class SuggestionJobTests : JobTest<SuggestionJob>
{
    private EmoteSuggestionService EmoteSuggestionService { get; set; }

    protected override SuggestionJob CreateJob()
    {
        var discordClient = DiscordHelper.CreateClient();
        var commandService = DiscordHelper.CreateCommandsService();
        var loggerFactory = LoggingHelper.CreateLoggerFactory();
        var interactionService = DiscordHelper.CreateInteractionService(discordClient);
        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);
        var client = new ClientBuilder().SetGetGuildsAction(Enumerable.Empty<IGuild>()).Build();
        var initManager = new InitManager(loggerFactory);
        initManager.Set(true);
        var suggestionSessionService = new SuggestionSessionService();
        var messageCacheManager = new MessageCacheManager(discordClient, initManager, CacheBuilder, TestServices.CounterManager.Value);
        EmoteSuggestionService = new EmoteSuggestionService(suggestionSessionService, DatabaseBuilder, client, messageCacheManager);
        var loggingManager = new LoggingManager(discordClient, commandService, interactionService, TestServices.EmptyProvider.Value);

        return new SuggestionJob(auditLogWriter, client, initManager, EmoteSuggestionService, suggestionSessionService, loggingManager);
    }

    [TestMethod]
    public async Task Execute_WithoutSessions()
    {
        var context = CreateContext();
        await Job.Execute(context);

        Assert.IsTrue(string.IsNullOrEmpty(context.Result as string));
    }

    [TestMethod]
    public async Task Execute_WithSessions()
    {
        EmoteSuggestionService.InitSession(Guid.NewGuid().ToString(), null);

        var context = CreateContext();
        await Job.Execute(context);

        Assert.IsTrue(string.IsNullOrEmpty(context.Result as string));
    }
}
