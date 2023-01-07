using Discord;
using GrillBot.App.Handlers.ChannelUpdated;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Handlers.ChannelUpdated;

[TestClass]
public class SyncChannelUpdatedHandlerTests : HandlerTest<SyncChannelUpdatedHandler>
{
    private ITextChannel TextChannel { get; set; }

    protected override SyncChannelUpdatedHandler CreateHandler()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        TextChannel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(guild).Build();

        return new SyncChannelUpdatedHandler(DatabaseBuilder);
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(TextChannel.Guild));
        await Repository.AddAsync(Database.Entity.GuildChannel.FromDiscord(TextChannel, ChannelType.Text));
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_Thread()
    {
        var thread = new ThreadBuilder(Consts.ThreadId, Consts.ThreadName).Build();
        await Handler.ProcessAsync(null, thread);
    }

    [TestMethod]
    public async Task ProcessAsync_Dms()
    {
        var channel = new DmChannelBuilder().Build();
        await Handler.ProcessAsync(null, channel);
    }

    [TestMethod]
    public async Task ProcessAsync_ChannelNotFound()
    {
        await Handler.ProcessAsync(TextChannel, TextChannel);
    }

    [TestMethod]
    public async Task ProcessAsync_Ok()
    {
        await InitDataAsync();
        await Handler.ProcessAsync(TextChannel, TextChannel);
    }
}
