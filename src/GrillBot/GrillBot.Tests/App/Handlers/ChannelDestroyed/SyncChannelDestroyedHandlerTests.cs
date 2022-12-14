using Discord;
using GrillBot.App.Handlers.ChannelDestroyed;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Handlers.ChannelDestroyed;

[TestClass]
public class SyncChannelDestroyedHandlerTests : HandlerTest<SyncChannelDestroyedHandler>
{
    private IGuild Guild { get; set; }
    private ITextChannel TextChannel { get; set; }
    private IThreadChannel Thread { get; set; }

    protected override SyncChannelDestroyedHandler CreateHandler()
    {
        Guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        TextChannel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(Guild).Build();
        Thread = new ThreadBuilder(Consts.ThreadId, Consts.ThreadName).SetParentChannel(TextChannel).SetGuild(Guild).Build();

        return new SyncChannelDestroyedHandler(DatabaseBuilder);
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.AddAsync(Database.Entity.GuildChannel.FromDiscord(TextChannel, ChannelType.Text));
        await Repository.AddAsync(Database.Entity.GuildChannel.FromDiscord(Thread, ChannelType.PrivateThread));
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_Thread()
    {
        await Handler.ProcessAsync(Thread);
    }

    [TestMethod]
    public async Task ProcessAsync_Dms()
    {
        var channel = new DmChannelBuilder().Build();
        await Handler.ProcessAsync(channel);
    }

    [TestMethod]
    public async Task ProcessAsync_ChannelNotFound()
    {
        await Handler.ProcessAsync(TextChannel);
    }

    [TestMethod]
    public async Task ProcessAsync_Ok()
    {
        await InitDataAsync();
        await Handler.ProcessAsync(TextChannel);
    }
}
