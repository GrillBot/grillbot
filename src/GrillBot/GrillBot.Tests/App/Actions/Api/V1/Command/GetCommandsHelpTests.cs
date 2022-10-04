using Discord;
using GrillBot.App.Actions.Api.V1.Command;
using GrillBot.App.Services.Channels;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Managers;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Common.Attributes;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Command;

[TestClass]
public class GetCommandsHelpTests : ApiActionTest<GetCommandsHelp>
{
    private ITextChannel TextChannel { get; set; }
    private MessageCacheManager MessageCacheManager { get; set; }

    protected override GetCommandsHelp CreateAction()
    {
        var guildBuilder = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName);
        var user = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guildBuilder.Build()).SetGuildPermissions(GuildPermissions.All).Build();
        var guild = guildBuilder.SetGetUserAction(user).SetGetCurrentUserAction(user).Build();
        var channelBuilder = new TextChannelBuilder().SetGuild(guild);
        var message = new UserMessageBuilder().SetId(Consts.MessageId).SetAuthor(user).SetChannel(channelBuilder.Build()).Build();
        TextChannel = channelBuilder.SetGetMessagesAsync(new[] { message }).Build();

        var client = new ClientBuilder().SetGetUserAction(user).SetGetGuildsAction(new[] { guild }).Build();
        var commandsService = DiscordHelper.CreateCommandsService(ServiceProvider);
        var discordClient = DiscordHelper.CreateClient();
        var initManager = new InitManager(TestServices.LoggerFactory.Value);
        MessageCacheManager = new MessageCacheManager(discordClient, initManager, CacheBuilder, TestServices.CounterManager.Value);
        var channelService = new ChannelService(discordClient, DatabaseBuilder, TestServices.Configuration.Value, MessageCacheManager);

        return new GetCommandsHelp(ApiRequestContext, client, commandsService, channelService, ServiceProvider, TestServices.Configuration.Value);
    }

    [TestMethod]
    [ApiConfiguration(canInitProvider: true)]
    public async Task ProcessAsync()
    {
        await MessageCacheManager.DownloadMessagesAsync(TextChannel);
        var result = await Action.ProcessAsync();

        Assert.IsTrue(result.Count > 0);
    }
}
