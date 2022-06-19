using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Logging;
using GrillBot.App.Services.Suggestion;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Counters;
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
        var loggingService = new LoggingService(discordClient, commandService, loggerFactory, configuration, DatabaseBuilder, interactionService);
        var initManager = new InitManager(LoggingHelper.CreateLoggerFactory());
        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);
        SessionService = new SuggestionSessionService();
        var emoteSuggestionService = new EmoteSuggestionService(SessionService, DatabaseBuilder);
        var featureSuggestionService = new FeatureSuggestionService(SessionService, configuration, DatabaseBuilder);
        var suggestionService = new SuggestionService(emoteSuggestionService, featureSuggestionService, dcClient, SessionService);

        initManager.Set(true);
        return new SuggestionJob(loggingService, auditLogWriter, discordClient, initManager, suggestionService, DatabaseBuilder);
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

        await Repository.AddAsync(new Database.Entity.Suggestion
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
        await Repository.AddAsync(guildEntity);
        await Repository.CommitAsync();

        var context = CreateContext();
        await Job.Execute(context);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task RunAsync_Process_FailedId()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();

        await Repository.AddAsync(new Database.Entity.Suggestion
        {
            BinaryData = new byte[] { 1, 2, 3, 4, 5 },
            BinaryDataFilename = "File.png",
            CreatedAt = System.DateTime.MinValue,
            Data = "Hello world",
            GuildId = guild.Id.ToString(),
            Type = SuggestionType.Emote
        });

        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(guild));
        await Repository.CommitAsync();

        var context = CreateContext();
        await Job.Execute(context);
        Assert.IsTrue(true);
    }
}
