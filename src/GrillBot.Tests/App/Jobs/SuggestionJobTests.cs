using Discord;
using GrillBot.App.Helpers;
using GrillBot.App.Jobs;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Managers;
using GrillBot.Tests.Infrastructure.Discord;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Tests.App.Jobs;

[TestClass]
public class SuggestionJobTests : JobTest<SuggestionJob>
{
    private IGuild Guild { get; set; } = null!;
    private IGuildUser User { get; set; } = null!;

    protected override SuggestionJob CreateJob()
    {
        var cacheManager = new EmoteSuggestionManager(CacheBuilder);
        var texts = TestServices.Texts.Value;
        var helper = new EmoteSuggestionHelper(texts);
        User = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var message = new UserMessageBuilder(Consts.MessageId).SetGetReactionUsersAction(new[] { User }).Build();
        var messageCache = new MessageCacheBuilder().SetGetAction(Consts.MessageId, message).Build();
        var suggestionChannel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).Build();
        Guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetTextChannelsAction(new[] { suggestionChannel }).Build();
        var client = new ClientBuilder()
            .SetGetGuildsAction(new[] { Guild }).SetGetUserAction(new[] { User })
            .Build();

        var provider = TestServices.InitializedProvider.Value;
        provider.GetRequiredService<InitManager>().Set(true);

        return new SuggestionJob(provider, cacheManager, helper, DatabaseBuilder, messageCache, client, texts);
    }

    private async Task InitDataAsync(ulong? voteChannelId, ulong? voteMessageId)
    {
        var guild = Database.Entity.Guild.FromDiscord(Guild);
        guild.EmoteSuggestionChannelId = Consts.ChannelId.ToString();
        guild.VoteChannelId = voteChannelId?.ToString();

        await Repository.AddAsync(guild);
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(Guild, User));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(new Database.Entity.EmoteSuggestion
        {
            Description = "Description",
            GuildId = Consts.GuildId.ToString(),
            Filename = "Filename.png",
            Id = 1,
            CreatedAt = DateTime.Now,
            ApprovedForVote = true,
            VoteFinished = false,
            VoteMessageId = voteMessageId?.ToString(),
            VoteEndsAt = DateTime.MinValue,
            SuggestionMessageId = Consts.MessageId.ToString(),
            ImageData = new byte[] { 1, 2, 3, 4, 5 },
            FromUserId = Consts.UserId.ToString(),
            EmoteName = "Emote"
        });
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task Execute_NoSuggestions()
    {
        var context = CreateContext();
        await Job.Execute(context);
    }

    [TestMethod]
    public async Task Execute_WithSuggestions()
    {
        await InitDataAsync(Consts.ChannelId, Consts.MessageId);
        var context = CreateContext();
        await Job.Execute(context);
    }

    [TestMethod]
    public async Task Execute_WithSuggestions_NoVoteChannel()
    {
        await InitDataAsync(null, Consts.MessageId);
        var context = CreateContext();
        await Job.Execute(context);
    }

    [TestMethod]
    public async Task Execute_WithSuggestions_VoteChannelNotFound()
    {
        await InitDataAsync(Consts.ChannelId + 1, Consts.MessageId);
        var context = CreateContext();
        await Job.Execute(context);
    }

    [TestMethod]
    public async Task Execute_WithSuggestions_MessageNotFound()
    {
        await InitDataAsync(Consts.ChannelId, Consts.MessageId + 50);
        var context = CreateContext();
        await Job.Execute(context);
    }
}
