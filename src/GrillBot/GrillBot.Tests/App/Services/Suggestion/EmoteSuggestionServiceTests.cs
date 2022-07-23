using Discord;
using GrillBot.App.Modules.Implementations.Suggestion;
using GrillBot.App.Services.Suggestion;
using GrillBot.Data.Exceptions;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;

namespace GrillBot.Tests.App.Services.Suggestion;

[TestClass]
public class EmoteSuggestionServiceTests : ServiceTest<EmoteSuggestionService>
{
    private IGuild Guild { get; set; }
    private IGuildUser User { get; set; }

    protected override EmoteSuggestionService CreateService()
    {
        var guildBuilder = new GuildBuilder()
            .SetIdentity(Consts.GuildId, Consts.GuildName);

        User = new GuildUserBuilder()
            .SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator)
            .SetGuild(guildBuilder.Build())
            .Build();

        var voteChannelBuilder = new TextChannelBuilder()
            .SetIdentity(Consts.ChannelId + 1, Consts.ChannelName)
            .SetGuild(guildBuilder.Build());

        var voteMessage = new UserMessageBuilder()
            .SetId(Consts.MessageId + 1)
            .SetGetReactionUsersAction(new[] { User })
            .SetAuthor(User)
            .SetChannel(voteChannelBuilder.Build())
            .Build();

        var suggestionChannel = new TextChannelBuilder().SetIdentity(Consts.ChannelId, Consts.ChannelName).SetGuild(guildBuilder.Build()).Build();
        var voteChannel = voteChannelBuilder.SetGetMessageAsync(voteMessage).Build();

        Guild = guildBuilder
            .SetGetTextChannelsAction(new[] { voteChannel, suggestionChannel })
            .Build();

        var discordClient = new ClientBuilder()
            .SetGetGuildsAction(new[] { Guild })
            .SetGetUserAction(User)
            .Build();

        var sesionService = new SuggestionSessionService();
        var discordSocketClient = DiscordHelper.CreateClient();
        var initManager = new InitManager(LoggingHelper.CreateLoggerFactory());
        var counterManager = new CounterManager();
        var messageCache = new MessageCacheManager(discordSocketClient, initManager, CacheBuilder, counterManager);

        return new EmoteSuggestionService(sesionService, DatabaseBuilder, discordClient, messageCache);
    }

    #region Process

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(NotFoundException))]
    public async Task ProcessSessionAsync_NoMetadata()
    {
        var suggestionId = Guid.NewGuid().ToString();
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var user = new GuildUserBuilder()
            .SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator)
            .SetGuild(guild)
            .Build();
        var modalData = new EmoteSuggestionModal();

        await Service.ProcessSessionAsync(suggestionId, guild, user, modalData);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public async Task ProcessSessionAsync_WithDescription_InvalidChannel()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var guildEntity = Database.Entity.Guild.FromDiscord(guild);
        guildEntity.EmoteSuggestionChannelId = "123456789";

        await Repository.AddAsync(guildEntity);
        await Repository.CommitAsync();

        var suggestionId = Guid.NewGuid().ToString();
        var user = new GuildUserBuilder()
            .SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator)
            .SetGuild(guild)
            .Build();

        var modalData = new EmoteSuggestionModal
        {
            EmoteDescription = "Popis",
            EmoteName = "Name"
        };

        Service.InitSession(suggestionId, Emote.Parse(Consts.OnlineEmoteId));
        await Service.ProcessSessionAsync(suggestionId, guild, user, modalData);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task ProcessSessionAsync_Attachment_Success()
    {
        const string filename = "File.png";

        var message = new UserMessageBuilder()
            .SetId(Consts.MessageId)
            .SetContent("Content")
            .Build();

        var channel = new TextChannelBuilder()
            .SetIdentity(Consts.ChannelId, Consts.ChannelName)
            .SetSendFileAction(filename, message)
            .Build();

        var guild = new GuildBuilder()
            .SetId(Consts.GuildId).SetName(Consts.GuildName)
            .SetGetTextChannelAction(channel)
            .Build();

        var guildEntity = Database.Entity.Guild.FromDiscord(guild);
        guildEntity.EmoteSuggestionChannelId = Consts.ChannelId.ToString();

        await Repository.AddAsync(guildEntity);
        await Repository.CommitAsync();

        var suggestionId = Guid.NewGuid().ToString();
        var user = new GuildUserBuilder()
            .SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator)
            .SetGuild(guild)
            .Build();
        var attachment = new AttachmentBuilder()
            .SetFilename(filename)
            .SetUrl("https://www.google.com/images/searchbox/desktop_searchbox_sprites318_hr.png")
            .Build();

        var modalData = new EmoteSuggestionModal { EmoteName = "Name" };

        Service.InitSession(suggestionId, attachment);
        await Service.ProcessSessionAsync(suggestionId, guild, user, modalData);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(GrillBotException))]
    public async Task ProcessSessionAsync_NoBinaryData()
    {
        var channel = new TextChannelBuilder()
            .SetIdentity(Consts.ChannelId, Consts.ChannelName)
            .Build();

        var guild = new GuildBuilder()
            .SetId(Consts.GuildId).SetName(Consts.GuildName)
            .SetGetTextChannelAction(channel)
            .Build();

        var guildEntity = Database.Entity.Guild.FromDiscord(guild);
        guildEntity.EmoteSuggestionChannelId = Consts.ChannelId.ToString();

        await Repository.AddAsync(guildEntity);
        await Repository.CommitAsync();

        var suggestionId = Guid.NewGuid().ToString();
        var user = new GuildUserBuilder()
            .SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator)
            .SetGuild(guild)
            .Build();

        var modalData = new EmoteSuggestionModal { EmoteName = "Name" };

        Service.InitSession(suggestionId, null);
        await Service.ProcessSessionAsync(suggestionId, guild, user, modalData);
        Assert.IsTrue(true);
    }

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
        var random = new Random();
        var buffer = new byte[100];
        random.NextBytes(buffer);

        var user = new GuildUserBuilder()
            .SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator)
            .Build();

        var guildData = Database.Entity.Guild.FromDiscord(Guild);
        guildData.EmoteSuggestionChannelId = Consts.ChannelId.ToString();

        await Repository.AddAsync(guildData);
        await Repository.AddAsync(Database.Entity.User.FromDiscord(user));
        await Repository.AddAsync(new EmoteSuggestion
        {
            Guild = guildData,
            VoteFinished = false,
            ApprovedForVote = true,
            VoteMessageId = (Consts.MessageId + 1).ToString(),
            VoteEndsAt = DateTime.Now.AddDays(-1),
            SuggestionMessageId = Consts.MessageId.ToString(),
            CreatedAt = DateTime.Now,
            ImageData = buffer,
            GuildId = guildData.Id,
            FromUserId = user.Id.ToString(),
            FromUser = GuildUser.FromDiscord(Guild, user),
            Filename = "File",
            EmoteName = Emote.Parse(Consts.PepeJamEmote).Name
        });

        await Repository.CommitAsync();

        var report = await Service.ProcessJobAsync();
        Assert.IsFalse(string.IsNullOrEmpty(report));
    }

    [TestMethod]
    public async Task ProcessJobAsync_MissingVoteChannel()
    {
        var random = new Random();
        var buffer = new byte[100];
        random.NextBytes(buffer);

        var user = new GuildUserBuilder()
            .SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator)
            .Build();

        var guildData = Database.Entity.Guild.FromDiscord(Guild);
        guildData.EmoteSuggestionChannelId = Consts.ChannelId.ToString();
        guildData.VoteChannelId = (Consts.ChannelId + 2).ToString();

        await Repository.AddAsync(guildData);
        await Repository.AddAsync(Database.Entity.User.FromDiscord(user));
        await Repository.AddAsync(new EmoteSuggestion
        {
            Guild = guildData,
            VoteFinished = false,
            ApprovedForVote = true,
            VoteMessageId = (Consts.MessageId + 1).ToString(),
            VoteEndsAt = DateTime.Now.AddDays(-1),
            SuggestionMessageId = Consts.MessageId.ToString(),
            CreatedAt = DateTime.Now,
            ImageData = buffer,
            GuildId = guildData.Id,
            FromUserId = user.Id.ToString(),
            FromUser = GuildUser.FromDiscord(Guild, user),
            Filename = "File",
            EmoteName = Emote.Parse(Consts.PepeJamEmote).Name
        });

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

        var user = new GuildUserBuilder()
            .SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator)
            .Build();

        var guildData = Database.Entity.Guild.FromDiscord(Guild);
        guildData.EmoteSuggestionChannelId = Consts.ChannelId.ToString();
        guildData.VoteChannelId = (Consts.ChannelId + 1).ToString();

        await Repository.AddAsync(guildData);
        await Repository.AddAsync(Database.Entity.User.FromDiscord(user));
        await Repository.AddAsync(new EmoteSuggestion
        {
            Guild = guildData,
            VoteFinished = false,
            ApprovedForVote = true,
            VoteMessageId = (Consts.MessageId + 3).ToString(),
            VoteEndsAt = DateTime.Now.AddDays(-1),
            SuggestionMessageId = Consts.MessageId.ToString(),
            CreatedAt = DateTime.Now,
            ImageData = buffer,
            GuildId = guildData.Id,
            FromUserId = user.Id.ToString(),
            FromUser = GuildUser.FromDiscord(Guild, user),
            Filename = "File",
            EmoteName = Emote.Parse(Consts.PepeJamEmote).Name
        });

        await Repository.CommitAsync();

        var report = await Service.ProcessJobAsync();
        Assert.IsFalse(string.IsNullOrEmpty(report));
    }

    [TestMethod]
    public async Task ProcessJobAsync_Finish()
    {
        var random = new Random();
        var buffer = new byte[100];
        random.NextBytes(buffer);

        var guildData = Database.Entity.Guild.FromDiscord(Guild);
        guildData.EmoteSuggestionChannelId = Consts.ChannelId.ToString();
        guildData.VoteChannelId = (Consts.ChannelId + 1).ToString();

        await Repository.AddAsync(guildData);
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(new EmoteSuggestion
        {
            Guild = guildData,
            VoteFinished = false,
            ApprovedForVote = true,
            VoteMessageId = (Consts.MessageId + 1).ToString(),
            VoteEndsAt = DateTime.Now.AddDays(-1),
            SuggestionMessageId = Consts.MessageId.ToString(),
            CreatedAt = DateTime.Now,
            ImageData = buffer,
            GuildId = guildData.Id,
            FromUserId = User.Id.ToString(),
            FromUser = GuildUser.FromDiscord(Guild, User),
            Filename = "File",
            EmoteName = Emote.Parse(Consts.PepeJamEmote).Name
        });

        await Repository.CommitAsync();

        var report = await Service.ProcessJobAsync();
        Assert.IsFalse(string.IsNullOrEmpty(report));
    }

    #endregion
}
