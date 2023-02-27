using Discord;
using Discord.Interactions;
using GrillBot.App.Handlers.InteractionCommandExecuted;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;
using Moq;

namespace GrillBot.Tests.App.Handlers.InteractionCommandExecuted;

[TestClass]
public class UpdateUserLanguageHandlerTests : TestBase<UpdateUserLanguageHandler>
{
    private static readonly IGuildUser User = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

    protected override UpdateUserLanguageHandler CreateInstance()
    {
        return new UpdateUserLanguageHandler(DatabaseBuilder);
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_NoData()
    {
        var command = new Mock<ICommandInfo>().Object;
        var interactionContext = new InteractionContextBuilder()
            .SetUser(User)
            .SetInteraction(new DiscordInteractionBuilder(Consts.InteractionId).SetUserLocale("cs").Build())
            .Build();

        await Instance.ProcessAsync(command, interactionContext, new ExecuteResult());
    }

    [TestMethod]
    public async Task ProcessDataAsync_WithData()
    {
        await InitDataAsync();

        var command = new Mock<ICommandInfo>().Object;
        var interactionContext = new InteractionContextBuilder()
            .SetUser(User)
            .SetInteraction(new DiscordInteractionBuilder(Consts.InteractionId).SetUserLocale("cs").Build())
            .Build();

        await Instance.ProcessAsync(command, interactionContext, new ExecuteResult());
    }
}
