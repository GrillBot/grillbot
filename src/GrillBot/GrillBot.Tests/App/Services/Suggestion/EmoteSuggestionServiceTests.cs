using Discord;
using GrillBot.App.Modules.Implementations.Suggestion;
using GrillBot.App.Services.Suggestion;
using GrillBot.Data.Exceptions;
using GrillBot.Tests.Infrastructure.Discord;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Managers;
using GrillBot.Database.Entity;

namespace GrillBot.Tests.App.Services.Suggestion;

[TestClass]
public class EmoteSuggestionServiceTests : ServiceTest<EmoteSuggestionService>
{
    private IGuild Guild { get; set; }
    private IGuildUser User { get; set; }
    private ITextChannel VoteChannel { get; set; }
    private ITextChannel SuggestionChannel { get; set; }
    private IUserMessage SuggestionMessage { get; set; }
    private IUserMessage VoteMessage { get; set; }

    protected override EmoteSuggestionService CreateService()
    {
        var guildBuilder = new GuildBuilder()
            .SetIdentity(Consts.GuildId, Consts.GuildName);

        User = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guildBuilder.Build()).Build();

        var voteChannelBuilder = new TextChannelBuilder().SetIdentity(Consts.ChannelId + 1, Consts.ChannelName).SetGuild(guildBuilder.Build());
        VoteMessage = new UserMessageBuilder().SetId(Consts.MessageId + 1).SetGetReactionUsersAction(new[] { User }).SetAuthor(User).SetChannel(voteChannelBuilder.Build()).Build();
        var suggestionChannelBuilder = new TextChannelBuilder().SetIdentity(Consts.ChannelId, Consts.ChannelName).SetGuild(guildBuilder.Build());
        SuggestionMessage = new UserMessageBuilder().SetId(Consts.MessageId).SetAuthor(User).SetChannel(suggestionChannelBuilder.Build()).Build();

        SuggestionChannel = suggestionChannelBuilder.SetGetMessageAsync(SuggestionMessage).Build();
        VoteChannel = voteChannelBuilder.SetGetMessageAsync(VoteMessage).SetSendFileAction("VoteStartedFile.png", VoteMessage).Build();

        Guild = guildBuilder.SetGetTextChannelsAction(new[] { VoteChannel, SuggestionChannel }).Build();

        var discordClient = new ClientBuilder().SetGetGuildsAction(new[] { Guild }).SetGetUserAction(User).Build();
        var sesionService = new SuggestionSessionService();
        var discordSocketClient = DiscordHelper.CreateClient();
        var initManager = new InitManager(LoggingHelper.CreateLoggerFactory());
        var messageCache = new MessageCacheManager(discordSocketClient, initManager, CacheBuilder, TestServices.CounterManager.Value);

        return new EmoteSuggestionService(sesionService, DatabaseBuilder, discordClient, messageCache);
    }

    #region Process

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(NotFoundException))]
    public async Task ProcessSessionAsync_NoMetadata()
    {
        var suggestionId = Guid.NewGuid().ToString();
        var modalData = new EmoteSuggestionModal();

        await Service.ProcessSessionAsync(suggestionId, Guild, User, modalData);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public async Task ProcessSessionAsync_WithDescription_InvalidChannel()
    {
        var guildEntity = Database.Entity.Guild.FromDiscord(Guild);
        guildEntity.EmoteSuggestionChannelId = "123456789";

        await Repository.AddAsync(guildEntity);
        await Repository.CommitAsync();

        var suggestionId = Guid.NewGuid().ToString();
        var modalData = new EmoteSuggestionModal
        {
            EmoteDescription = "Popis",
            EmoteName = "Name"
        };

        Service.InitSession(suggestionId, Emote.Parse(Consts.OnlineEmoteId));
        await Service.ProcessSessionAsync(suggestionId, Guild, User, modalData);
    }

    [TestMethod]
    public async Task ProcessSessionAsync_Attachment_Success()
    {
        const string filename = "File.png";

        var message = new UserMessageBuilder().SetId(Consts.MessageId).SetContent("Content").Build();
        var channel = new TextChannelBuilder().SetIdentity(Consts.ChannelId, Consts.ChannelName).SetSendFileAction(filename, message).Build();
        var guild = new GuildBuilder().SetId(Consts.GuildId).SetName(Consts.GuildName).SetGetTextChannelAction(channel).Build();

        var guildEntity = Database.Entity.Guild.FromDiscord(guild);
        guildEntity.EmoteSuggestionChannelId = Consts.ChannelId.ToString();

        await Repository.AddAsync(guildEntity);
        await Repository.CommitAsync();

        var suggestionId = Guid.NewGuid().ToString();
        var user = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var attachment = new AttachmentBuilder().SetFilename(filename).SetUrl("https://www.google.com/images/searchbox/desktop_searchbox_sprites318_hr.png").Build();

        var modalData = new EmoteSuggestionModal { EmoteName = "Name" };

        Service.InitSession(suggestionId, attachment);
        await Service.ProcessSessionAsync(suggestionId, guild, user, modalData);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(GrillBotException))]
    public async Task ProcessSessionAsync_NoBinaryData()
    {
        var guildEntity = Database.Entity.Guild.FromDiscord(Guild);
        guildEntity.EmoteSuggestionChannelId = Consts.ChannelId.ToString();

        await Repository.AddAsync(guildEntity);
        await Repository.CommitAsync();

        var suggestionId = Guid.NewGuid().ToString();
        var modalData = new EmoteSuggestionModal { EmoteName = "Name" };

        Service.InitSession(suggestionId, null);
        await Service.ProcessSessionAsync(suggestionId, Guild, User, modalData);
        Assert.IsTrue(true);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(GrillBotException))]
    public async Task ProcessSessionAsync_NoBinaryDataFromAttachment()
    {
        var guildEntity = Database.Entity.Guild.FromDiscord(Guild);
        guildEntity.EmoteSuggestionChannelId = Consts.ChannelId.ToString();

        await Repository.AddAsync(guildEntity);
        await Repository.CommitAsync();

        var suggestionId = Guid.NewGuid().ToString();
        var modalData = new EmoteSuggestionModal { EmoteName = "Name" };

        var attachment = new AttachmentBuilder()
            .SetFilename("File.png")
            .SetUrl("https://ThisUrlIsRefused:8080")
            .Build();

        Service.InitSession(suggestionId, attachment);
        await Service.ProcessSessionAsync(suggestionId, Guild, User, modalData);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task ProcessSessionAsync_NoGuildData()
    {
        var suggestionId = Guid.NewGuid().ToString();
        var modalData = new EmoteSuggestionModal
        {
            EmoteDescription = "Popis",
            EmoteName = "Name"
        };

        Service.InitSession(suggestionId, Emote.Parse(Consts.OnlineEmoteId));
        await Service.ProcessSessionAsync(suggestionId, Guild, User, modalData);
    }

    #region ProcessSuggestionsToVoteAsync

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessSuggestionsToVoteAsync_NotSetSuggestionChannel()
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.CommitAsync();

        await Service.ProcessSuggestionsToVoteAsync(Guild);
    }

    [TestMethod]
    [ExpectedException(typeof(GrillBotException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessSuggestionsToVoteAsync_NoVoteChannel()
    {
        var guildData = Database.Entity.Guild.FromDiscord(Guild);
        guildData.EmoteSuggestionChannelId = SuggestionChannel.Id.ToString();

        await Repository.AddAsync(guildData);
        await Repository.CommitAsync();

        await Service.ProcessSuggestionsToVoteAsync(Guild);
    }

    [TestMethod]
    [ExpectedException(typeof(GrillBotException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessSuggestionsToVoteAsync_MissingVoteChannel()
    {
        var guildData = Database.Entity.Guild.FromDiscord(Guild);
        guildData.EmoteSuggestionChannelId = SuggestionChannel.Id.ToString();
        guildData.VoteChannelId = (VoteChannel.Id + 3).ToString();

        await Repository.AddAsync(guildData);
        await Repository.CommitAsync();

        await Service.ProcessSuggestionsToVoteAsync(Guild);
    }

    [TestMethod]
    [ExpectedException(typeof(GrillBotException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessSuggestionsToVoteAsync_NoSuggestion()
    {
        var guildData = Database.Entity.Guild.FromDiscord(Guild);
        guildData.EmoteSuggestionChannelId = SuggestionChannel.Id.ToString();
        guildData.VoteChannelId = VoteChannel.Id.ToString();

        await Repository.AddAsync(guildData);
        await Repository.CommitAsync();

        await Service.ProcessSuggestionsToVoteAsync(Guild);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessSuggestionsToVoteAsync_NoApproved()
    {
        var guildData = Database.Entity.Guild.FromDiscord(Guild);
        guildData.EmoteSuggestionChannelId = SuggestionChannel.Id.ToString();
        guildData.VoteChannelId = VoteChannel.Id.ToString();

        await Repository.AddAsync(CreateEntity(approvedForVote: false, guildData: guildData));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(guildData);
        await Repository.CommitAsync();

        await Service.ProcessSuggestionsToVoteAsync(Guild);
    }

    [TestMethod]
    public async Task ProcessSuggestionsToVoteAsync_Finished_ButNoMessage()
    {
        var guildData = Database.Entity.Guild.FromDiscord(Guild);
        guildData.EmoteSuggestionChannelId = SuggestionChannel.Id.ToString();
        guildData.VoteChannelId = VoteChannel.Id.ToString();

        await Repository.AddAsync(CreateEntity(approvedForVote: true, guildData: guildData, filename: "VoteStartedFile.png", suggestionMessageId: Consts.MessageId + 1));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(guildData);
        await Repository.CommitAsync();

        await Service.ProcessSuggestionsToVoteAsync(Guild);
    }

    [TestMethod]
    public async Task ProcessSuggestionsToVoteAsync_Finished()
    {
        var guildData = Database.Entity.Guild.FromDiscord(Guild);
        guildData.EmoteSuggestionChannelId = SuggestionChannel.Id.ToString();
        guildData.VoteChannelId = VoteChannel.Id.ToString();

        await Repository.AddAsync(CreateEntity(approvedForVote: true, guildData: guildData, filename: "VoteStartedFile.png", suggestionMessageId: Consts.MessageId));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(guildData);
        await Repository.CommitAsync();

        await Service.ProcessSuggestionsToVoteAsync(Guild);
    }

    #endregion

    #endregion

    #region Job

    [TestMethod]
    public async Task ProcessJobAsync_NoData()
    {
        var report = await Service.ProcessJobAsync();
        Assert.IsTrue(string.IsNullOrEmpty(report));
    }

    [TestMethod]
    public async Task ProcessJobAsync_NoVoteChannel()
    {
        var guildData = Database.Entity.Guild.FromDiscord(Guild);
        guildData.EmoteSuggestionChannelId = Consts.ChannelId.ToString();

        await Repository.AddAsync(guildData);
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));

        await Repository.AddAsync(CreateEntity(Consts.MessageId + 1, false, true, DateTime.Now.AddDays(-1), guildData: guildData));
        await Repository.CommitAsync();

        var report = await Service.ProcessJobAsync();
        Assert.IsFalse(string.IsNullOrEmpty(report));
    }

    [TestMethod]
    public async Task ProcessJobAsync_MissingVoteChannel()
    {
        var guildData = Database.Entity.Guild.FromDiscord(Guild);
        guildData.EmoteSuggestionChannelId = Consts.ChannelId.ToString();
        guildData.VoteChannelId = (Consts.ChannelId + 2).ToString();

        await Repository.AddAsync(guildData);
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(CreateEntity(guildData: guildData, voteFinished: false, approvedForVote: true, voteEndsAt: DateTime.Now.AddDays(-1),
            voteMessageId: Consts.MessageId + 1));
        await Repository.CommitAsync();

        var report = await Service.ProcessJobAsync();
        Assert.IsFalse(string.IsNullOrEmpty(report));
    }

    [TestMethod]
    public async Task ProcessJobAsync_NoMessage()
    {
        var random = new Random();
        var buffer = new byte[100];
        random.NextBytes(buffer);

        var guildData = Database.Entity.Guild.FromDiscord(Guild);
        guildData.EmoteSuggestionChannelId = Consts.ChannelId.ToString();
        guildData.VoteChannelId = (Consts.ChannelId + 1).ToString();

        await Repository.AddAsync(guildData);
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(CreateEntity(guildData: guildData, voteFinished: false, approvedForVote: true, voteMessageId: Consts.MessageId + 3, voteEndsAt: DateTime.Now.AddDays(-1)));
        await Repository.CommitAsync();

        var report = await Service.ProcessJobAsync();
        Assert.IsFalse(string.IsNullOrEmpty(report));
    }

    [TestMethod]
    public async Task ProcessJobAsync_Finish()
    {
        var guildData = Database.Entity.Guild.FromDiscord(Guild);
        guildData.EmoteSuggestionChannelId = Consts.ChannelId.ToString();
        guildData.VoteChannelId = (Consts.ChannelId + 1).ToString();

        await Repository.AddAsync(guildData);
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(CreateEntity(guildData: guildData, voteFinished: false, approvedForVote: true, voteMessageId: Consts.MessageId + 1, voteEndsAt: DateTime.Now.AddDays(-1)));
        await Repository.CommitAsync();

        var report = await Service.ProcessJobAsync();
        Assert.IsFalse(string.IsNullOrEmpty(report));
    }

    #endregion

    #region Approval

    [TestMethod]
    public async Task SetApprovalStateAsync_NoSuggestion()
    {
        var interaction = new ComponentInteractionBuilder().SetGuild(Guild).SetMessage(SuggestionMessage).Build();

        await Service.SetApprovalStateAsync(interaction, true, SuggestionChannel);
    }

    [TestMethod]
    public async Task SetApprovalState_VoteFinished()
    {
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(CreateEntity(approvedForVote: true, voteFinished: true, voteEndsAt: DateTime.MaxValue));
        await Repository.CommitAsync();

        var interaction = new ComponentInteractionBuilder().SetGuild(Guild).SetMessage(SuggestionMessage).Build();
        await Service.SetApprovalStateAsync(interaction, false, SuggestionChannel);

        var suggestion = await Repository.EmoteSuggestion.FindSuggestionByMessageId(Guild.Id, SuggestionMessage.Id);
        Assert.IsNotNull(suggestion);
        Assert.IsTrue(suggestion.ApprovedForVote);
    }

    [TestMethod]
    public async Task SetApprovalState_Approved_NoMessage()
    {
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(CreateEntity(suggestionMessageId: VoteMessage.Id));
        await Repository.CommitAsync();

        var interaction = new ComponentInteractionBuilder().SetGuild(Guild).SetMessage(VoteMessage).Build();
        await Service.SetApprovalStateAsync(interaction, false, SuggestionChannel);

        Repository.ClearChangeTracker();
        var suggestion = await Repository.EmoteSuggestion.FindSuggestionByMessageId(Guild.Id, VoteMessage.Id);
        Assert.IsNotNull(suggestion);
        Assert.IsFalse(suggestion.ApprovedForVote);
    }

    #endregion

    #region Events

    [TestMethod]
    public async Task OnMessageDeletedAsync_SuggestionNotFound()
    {
        await Service.OnMessageDeletedAsync(SuggestionMessage, Guild);
    }

    [TestMethod]
    public async Task OnMessageDeletedAsync_VoteFinished()
    {
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(CreateEntity(approvedForVote: true, voteFinished: true, voteEndsAt: DateTime.MaxValue));
        await Repository.CommitAsync();

        await Service.OnMessageDeletedAsync(SuggestionMessage, Guild);
    }

    [TestMethod]
    public async Task OnMessageDeletedAsync_VoteRunning()
    {
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(CreateEntity(approvedForVote: true, voteFinished: false, voteEndsAt: DateTime.MaxValue));
        await Repository.CommitAsync();

        await Service.OnMessageDeletedAsync(SuggestionMessage, Guild);
    }

    [TestMethod]
    public async Task OnMessageDeletedAsync_Success()
    {
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(CreateEntity(approvedForVote: true));
        await Repository.CommitAsync();

        await Service.OnMessageDeletedAsync(SuggestionMessage, Guild);
    }

    #endregion

    #region Misc

    private EmoteSuggestion CreateEntity(ulong? voteMessageId = default, bool voteFinished = default, bool? approvedForVote = default, DateTime? voteEndsAt = default,
        string filename = "File", Database.Entity.Guild guildData = null, ulong suggestionMessageId = 0)
    {
        return new EmoteSuggestion
        {
            ApprovedForVote = approvedForVote,
            Filename = filename,
            Guild = guildData ?? Database.Entity.Guild.FromDiscord(Guild),
            CreatedAt = DateTime.Now,
            EmoteName = Emote.Parse(Consts.PepeJamEmote).Name,
            FromUser = GuildUser.FromDiscord(Guild, User),
            GuildId = Guild.Id.ToString(),
            ImageData = new byte[] { 1 },
            FromUserId = User.Id.ToString(),
            SuggestionMessageId = suggestionMessageId > 0 ? suggestionMessageId.ToString() : Consts.MessageId.ToString(),
            VoteMessageId = voteMessageId?.ToString(),
            VoteFinished = voteFinished,
            VoteEndsAt = voteEndsAt
        };
    }

    #endregion
}
