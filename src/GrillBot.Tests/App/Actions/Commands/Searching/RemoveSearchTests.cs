using Discord;
using GrillBot.App.Actions.Commands.Searching;
using GrillBot.App.Managers;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands.Searching;

[TestClass]
public class RemoveSearchTests : CommandActionTest<RemoveSearch>
{
    private static readonly IGuild GuildData = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();

    protected override IGuildUser User => new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuildPermissions(GuildPermissions.All).Build();
    protected override IGuild Guild => GuildData;
    protected override IMessageChannel Channel => new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(GuildData).Build();

    protected override RemoveSearch CreateInstance()
    {
        var userManager = new UserManager(DatabaseBuilder);
        return InitAction(new RemoveSearch(userManager, DatabaseBuilder, TestServices.Texts.Value));
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.AddAsync(Database.Entity.GuildChannel.FromDiscord((IGuildChannel)Channel, ChannelType.Text));

        await Repository.AddAsync(new Database.Entity.SearchItem
        {
            UserId = Consts.UserId.ToString(),
            MessageContent = "Test",
            ChannelId = Consts.ChannelId.ToString(),
            GuildId = Consts.GuildId.ToString(),
            Id = 1
        });

        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_NotFound()
    {
        await Instance.ProcessAsync(1);
        Assert.IsNull(Instance.ErrorMessage);
    }

    [TestMethod]
    public async Task ProcessAsync_Ok()
    {
        await InitDataAsync();
        await Instance.ProcessAsync(1);

        Assert.IsNull(Instance.ErrorMessage);
    }
}
