using Discord;
using GrillBot.App.Handlers.ChannelDestroyed;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Handlers.ChannelDestroyed;

[TestClass]
public class SyncChannelDestroyedHandlerTests : TestBase<SyncChannelDestroyedHandler>
{
    private IGuild Guild { get; set; } = null!;
    private ITextChannel TextChannel { get; set; } = null!;
    private IThreadChannel Thread { get; set; } = null!;

    protected override void PreInit()
    {
        Guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        TextChannel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(Guild).Build();
        Thread = new ThreadBuilder(Consts.ThreadId, Consts.ThreadName).SetParentChannel(TextChannel).SetGuild(Guild).Build();
    }

    protected override SyncChannelDestroyedHandler CreateInstance()
    {
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
        await Instance.ProcessAsync(Thread);
    }

    [TestMethod]
    public async Task ProcessAsync_Dms()
    {
        var channel = new DmChannelBuilder().Build();
        await Instance.ProcessAsync(channel);
    }

    [TestMethod]
    public async Task ProcessAsync_ChannelNotFound()
    {
        await Instance.ProcessAsync(TextChannel);
    }

    [TestMethod]
    public async Task ProcessAsync_Ok()
    {
        await InitDataAsync();
        await Instance.ProcessAsync(TextChannel);
    }
}
