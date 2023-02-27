using System.Diagnostics.CodeAnalysis;
using Discord;
using GrillBot.App.Actions.Commands.EmoteSuggestion;
using GrillBot.App.Helpers;
using GrillBot.Data.Exceptions;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands.EmoteSuggestion;

[TestClass]
public class ProcessToVoteTests : CommandActionTest<ProcessToVote>
{
    private static readonly IUserMessage Message = new UserMessageBuilder(Consts.MessageId).Build();

    protected override IGuild Guild
        => new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetTextChannelsAction(new[]
        {
            new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetSendFileAction("Filename.png", Message).Build(), // VoteChannel
            new TextChannelBuilder(Consts.ChannelId + 1, Consts.ChannelName + "Suggestion").Build() // SuggestionChannel
        }).Build();

    protected override IGuildUser User
        => new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

    protected override ProcessToVote CreateInstance()
    {
        var texts = TestServices.Texts.Value;
        var helper = new EmoteSuggestionHelper(texts);
        var messageCache = new MessageCacheBuilder()
            .SetGetAction(Message.Id, Message)
            .Build();
        var client = new ClientBuilder()
            .SetGetUserAction(User)
            .Build();

        return InitAction(new ProcessToVote(DatabaseBuilder, texts, helper, messageCache, client));
    }

    private async Task InitDataAsync(ulong? voteChannelId, ulong? emoteSuggestionChannelId, bool? approved)
    {
        var guildEntity = Database.Entity.Guild.FromDiscord(Guild);
        guildEntity.VoteChannelId = voteChannelId?.ToString();
        guildEntity.EmoteSuggestionChannelId = emoteSuggestionChannelId?.ToString();
        await Repository.AddAsync(guildEntity);

        if (approved != null)
        {
            await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
            await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(Guild, User));
            await Repository.AddAsync(new Database.Entity.EmoteSuggestion
            {
                CreatedAt = DateTime.Now,
                EmoteName = "Emote",
                Description = "Description",
                GuildId = Consts.GuildId.ToString(),
                Filename = "Filename.png",
                Id = 1,
                ImageData = new byte[] { 1, 2, 3, 4, 5 },
                FromUserId = Consts.UserId.ToString(),
                ApprovedForVote = approved,
                SuggestionMessageId = Consts.MessageId.ToString()
            });
        }

        await Repository.CommitAsync();
    }

    [TestMethod]
    [ExpectedException(typeof(GrillBotException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_VoteChannelNotDefined()
    {
        await InitDataAsync(null, null, null);
        await Instance.ProcessAsync();
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_VoteChannelNotFound()
    {
        await InitDataAsync(Consts.ChannelId + 50, null, null);
        await Instance.ProcessAsync();
    }

    [TestMethod]
    [ExpectedException(typeof(GrillBotException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_NoForVote()
    {
        await InitDataAsync(Consts.ChannelId, Consts.ChannelId + 1, null);
        await Instance.ProcessAsync();
    }

    [TestMethod]
    [ExpectedException(typeof(GrillBotException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_NoApproved()
    {
        await InitDataAsync(Consts.ChannelId, Consts.ChannelId + 1, false);
        await Instance.ProcessAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        await InitDataAsync(Consts.ChannelId, Consts.ChannelId + 1, true);
        await Instance.ProcessAsync();
    }
}
