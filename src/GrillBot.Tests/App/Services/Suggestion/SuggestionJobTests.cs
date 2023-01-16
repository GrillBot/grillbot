using System.Linq;
using Discord;
using GrillBot.App.Services.Suggestion;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Managers;
using GrillBot.Tests.Infrastructure.Discord;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Tests.App.Services.Suggestion;

[TestClass]
public class SuggestionJobTests : JobTest<SuggestionJob>
{
    private EmoteSuggestionService EmoteSuggestionService { get; set; }

    protected override SuggestionJob CreateJob()
    {
        var serviceProvider = TestServices.InitializedProvider.Value;
        var initManager = serviceProvider.GetRequiredService<InitManager>();
    
        var client = new ClientBuilder().SetGetGuildsAction(Enumerable.Empty<IGuild>()).Build();
        initManager.Set(true);
        var suggestionSessionService = new SuggestionSessionService();
        var messageCacheManager = new MessageCacheManager(TestServices.DiscordSocketClient.Value, initManager, CacheBuilder, TestServices.CounterManager.Value);
        EmoteSuggestionService = new EmoteSuggestionService(suggestionSessionService, DatabaseBuilder, client, messageCacheManager);

        return new SuggestionJob(EmoteSuggestionService, suggestionSessionService, serviceProvider);
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
