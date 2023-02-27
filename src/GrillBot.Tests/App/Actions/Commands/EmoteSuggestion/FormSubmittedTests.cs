using Discord;
using GrillBot.App.Actions.Commands.EmoteSuggestion;
using GrillBot.App.Helpers;
using GrillBot.App.Modules.Implementations.Suggestion;
using GrillBot.Cache.Services.Managers;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands.EmoteSuggestion;

[TestClass]
public class FormSubmittedTests : CommandActionTest<FormSubmitted>
{
    private static readonly IUserMessage Message = new UserMessageBuilder(Consts.MessageId).Build();

    private static readonly IGuild GuildData = new GuildBuilder(Consts.GuildId, Consts.GuildName)
        .SetGetTextChannelsAction(new[]
        {
            new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetSendFileAction("Filename.png", Message).Build()
        })
        .Build();

    private string SessionId { get; set; } = null!;
    protected override IGuildUser User => new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetSendMessageAction(null, true).SetGuild(GuildData).Build();
    protected override IGuild Guild => GuildData;

    protected override FormSubmitted CreateInstance()
    {
        SessionId = Guid.NewGuid().ToString();

        var cacheManager = new EmoteSuggestionManager(CacheBuilder);
        var texts = TestServices.Texts.Value;
        var helper = new EmoteSuggestionHelper(texts);

        return InitAction(new FormSubmitted(cacheManager, texts, DatabaseBuilder, helper));
    }

    private async Task InitDataAsync()
    {
        await CacheRepository.AddAsync(new Cache.Entity.EmoteSuggestionMetadata
        {
            Filename = "Filename.png",
            Id = SessionId,
            CreatedAt = DateTime.Now,
            DataContent = new byte[] { 0, 1, 2, 3, 4, 5 }
        });
        await CacheRepository.CommitAsync();

        var guildEntity = Database.Entity.Guild.FromDiscord(GuildData);
        guildEntity.EmoteSuggestionChannelId = Consts.ChannelId.ToString();

        await Repository.AddAsync(guildEntity);
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_SessionNotFound()
        => await Instance.ProcessAsync(SessionId, new EmoteSuggestionModal());

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        var modal = new EmoteSuggestionModal
        {
            EmoteDescription = "Description",
            EmoteName = "EmoteName"
        };

        await InitDataAsync();
        await Instance.ProcessAsync(SessionId, modal);
    }
}
