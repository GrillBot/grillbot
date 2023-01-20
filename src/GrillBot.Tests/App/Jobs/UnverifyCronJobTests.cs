using GrillBot.App.Actions.Api.V1.Unverify;
using GrillBot.App.Jobs;
using GrillBot.App.Managers;
using GrillBot.App.Services.Unverify;
using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Logging;
using GrillBot.Tests.Infrastructure.Discord;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Tests.App.Jobs;

[TestClass]
public class UnverifyCronJobTests : JobTest<UnverifyCronJob>
{
    protected override UnverifyCronJob CreateJob()
    {
        var client = new ClientBuilder().Build();
        var texts = TestServices.Texts.Value;
        var messageGenerator = new UnverifyMessageManager(texts);
        var logger = new UnverifyLogManager(client, DatabaseBuilder);
        var commandService = DiscordHelper.CreateCommandsService();
        var discordClient = TestServices.DiscordSocketClient.Value;
        var interaction = DiscordHelper.CreateInteractionService(discordClient);
        var provider = TestServices.InitializedProvider.Value;
        provider.GetRequiredService<InitManager>().Set(true);
        var logging = new LoggingManager(discordClient, commandService, interaction, provider);
        var unverifyHelper = new UnverifyHelper(DatabaseBuilder);
        var removeUnverify = new RemoveUnverify(new ApiRequestContext(), client, texts, DatabaseBuilder, messageGenerator, logger, logging, unverifyHelper);
        return new UnverifyCronJob(provider, removeUnverify, DatabaseBuilder);
    }

    [TestMethod]
    public async Task ProcessAsync_NoUnverify()
    {
        var context = CreateContext();
        await Job.Execute(context);
    }
}
