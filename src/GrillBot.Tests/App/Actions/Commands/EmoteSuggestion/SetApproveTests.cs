using Discord;
using GrillBot.App.Actions.Commands.EmoteSuggestion;
using GrillBot.App.Helpers;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands.EmoteSuggestion;

[TestClass]
public class SetApproveTests : CommandActionTest<SetApprove>
{
    private static readonly IUserMessage Message = new UserMessageBuilder(Consts.MessageId).Build();

    protected override IDiscordInteraction Interaction => new ComponentInteractionBuilder(Consts.InteractionId).SetMessage(Message).Build();
    protected override IGuild Guild => new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
    protected override IGuildUser User => new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

    protected override SetApprove CreateInstance()
    {
        var messageCache = new MessageCacheBuilder().SetGetAction(Message.Id, Message).Build();
        var client = new ClientBuilder().SetGetUserAction(User).Build();
        var texts = TestServices.Texts.Value;
        var helper = new EmoteSuggestionHelper(texts);

        return InitAction(new SetApprove(DatabaseBuilder, messageCache, client, texts, helper));
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(Guild, User));
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));

        await Repository.AddAsync(new Database.Entity.EmoteSuggestion
        {
            GuildId = Consts.GuildId.ToString(),
            Id = 1,
            Description = "Description",
            Filename = "Filename.png",
            CreatedAt = DateTime.Now,
            ImageData = new byte[] { 1, 2, 3, 4, 5 },
            EmoteName = "Emote",
            FromUserId = Consts.UserId.ToString(),
            SuggestionMessageId = Consts.MessageId.ToString()
        });
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_NoSuggestion()
        => await Instance.ProcessAsync(false);

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        await InitDataAsync();
        await Instance.ProcessAsync(true);
    }
}
