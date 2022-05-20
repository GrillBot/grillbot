using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.Logging;
using GrillBot.App.Services.MessageCache;
using GrillBot.App.Services.Suggestion;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Services.Suggestion;

[TestClass]
public class SuggestionJobTests : JobTest<SuggestionJob>
{
    private SuggestionSessionService SessionService { get; set; }

    protected override SuggestionJob CreateJob()
    {
        var dcClient = new ClientBuilder().Build();
        var discordClient = DiscordHelper.CreateClient();
        var commandService = DiscordHelper.CreateCommandsService();
        var configuration = ConfigurationHelper.CreateConfiguration();
        var loggerFactory = LoggingHelper.CreateLoggerFactory();
        var interactionService = DiscordHelper.CreateInteractionService(discordClient);
        var loggingService = new LoggingService(discordClient, commandService, loggerFactory, configuration, DbFactory, interactionService);
        var initializationService = new DiscordInitializationService(LoggingHelper.CreateLogger<DiscordInitializationService>());
        var messageCache = new MessageCache(discordClient, initializationService, CacheBuilder);
        var fileStorage = FileStorageHelper.Create(configuration);
        var auditLogService = new AuditLogService(discordClient, DbFactory, messageCache, fileStorage, initializationService);
        SessionService = new SuggestionSessionService();
        var emoteSuggestionService = new EmoteSuggestionService(SessionService, DbFactory);
        var featureSuggestionService = new FeatureSuggestionService(SessionService, configuration, DbFactory);
        var suggestionService = new SuggestionService(emoteSuggestionService, featureSuggestionService, dcClient, SessionService);

        initializationService.Set(true);
        return new SuggestionJob(loggingService, auditLogService, discordClient, initializationService, suggestionService, DbFactory);
    }

    [TestMethod]
    public async Task RunAsync_NoToPending()
    {
        var context = CreateContext();

        await Job.Execute(context);
        Assert.IsNull(context.Result);
    }

    [TestMethod]
    public async Task RunAsync_OnlyPurge()
    {
        SessionService.InitSuggestion("A", SuggestionType.Emote, "B");

        var context = CreateContext();

        await Job.Execute(context);
        Assert.IsNull(context.Result);
    }

    [TestMethod]
    public async Task RunAsync_Process()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();

        await DbContext.Suggestions.AddAsync(new Database.Entity.Suggestion()
        {
            BinaryData = new byte[] { 1, 2, 3, 4, 5 },
            BinaryDataFilename = "File.png",
            CreatedAt = System.DateTime.MinValue,
            Data = "Hello world",
            GuildId = guild.Id.ToString(),
            Id = 1,
            Type = SuggestionType.Emote
        });

        var guildEntity = Database.Entity.Guild.FromDiscord(guild);
        guildEntity.EmoteSuggestionChannelId = Consts.ChannelId.ToString();
        await DbContext.Guilds.AddAsync(guildEntity);
        await DbContext.SaveChangesAsync();

        var context = CreateContext();
        await Job.Execute(context);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task RunAsync_Process_FailedId()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();

        await DbContext.Suggestions.AddAsync(new Database.Entity.Suggestion()
        {
            BinaryData = new byte[] { 1, 2, 3, 4, 5 },
            BinaryDataFilename = "File.png",
            CreatedAt = System.DateTime.MinValue,
            Data = "Hello world",
            GuildId = guild.Id.ToString(),
            Type = SuggestionType.Emote
        });

        await DbContext.Guilds.AddAsync(Database.Entity.Guild.FromDiscord(guild));
        await DbContext.SaveChangesAsync();

        var context = CreateContext();
        await Job.Execute(context);
        Assert.IsTrue(true);
    }
}
