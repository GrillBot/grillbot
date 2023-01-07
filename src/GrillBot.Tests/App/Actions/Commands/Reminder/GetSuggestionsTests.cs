using Discord;
using GrillBot.App.Actions.Commands.Reminder;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands.Reminder;

[TestClass]
public class GetSuggestionsTests : CommandActionTest<GetSuggestions>
{
    protected override IGuildUser User
        => new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

    protected override GetSuggestions CreateAction()
    {
        return InitAction(new GetSuggestions(DatabaseBuilder, TestServices.Texts.Value));
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(new Database.Entity.RemindMessage
        {
            At = DateTime.Now,
            FromUser = Database.Entity.User.FromDiscord(User),
            FromUserId = User.Id.ToString(),
            Id = 1,
            Message = "Message",
            OriginalMessageId = "12345",
            ToUser = Database.Entity.User.FromDiscord(User),
            ToUserId = User.Id.ToString(),
            Language = "cs"
        });
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync()
    {
        await InitDataAsync();

        var suggestions = await Action.ProcessAsync();
        Assert.AreEqual(1, suggestions.Count);
        Assert.AreEqual(1L, suggestions[0].Value);
    }
}
