using System;
using System.Linq;
using Discord;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Logging;
using GrillBot.App.Services.Suggestion;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Counters;
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
        var configuration = ConfigurationHelper.CreateConfiguration();
        var interactionService = DiscordHelper.CreateInteractionService(discordClient);
        var loggingService = new LoggingService(discordClient, commandService, loggerFactory, configuration, DatabaseBuilder, interactionService);
        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);
        var client = new ClientBuilder().SetGetGuildsAction(Enumerable.Empty<IGuild>()).Build();
        var initManager = new InitManager(loggerFactory);
        initManager.Set(true);
        var suggestionSessionService = new SuggestionSessionService();
        var counterManager = new CounterManager();
        var messageCacheManager = new MessageCacheManager(discordClient, initManager, CacheBuilder, counterManager);
        EmoteSuggestionService = new EmoteSuggestionService(suggestionSessionService, DatabaseBuilder, client, messageCacheManager);

        return new SuggestionJob(loggingService, auditLogWriter, client, initManager, EmoteSuggestionService, suggestionSessionService);
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
