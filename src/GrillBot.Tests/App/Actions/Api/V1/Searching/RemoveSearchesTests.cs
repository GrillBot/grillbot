using Discord;
using GrillBot.App.Actions.Api.V1.Searching;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Searching;

[TestClass]
public class RemoveSearchesTests : ApiActionTest<RemoveSearches>
{
    protected override RemoveSearches CreateInstance()
    {
        return new RemoveSearches(ApiRequestContext, DatabaseBuilder);
    }

    private async Task InitDataAsync()
    {
        var guildBuilder = new GuildBuilder(Consts.GuildId, Consts.GuildName);
        var user = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guildBuilder.Build()).Build();
        var textChannel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(guildBuilder.Build()).Build();
        var guild = guildBuilder.SetGetUsersAction(new[] { user }).SetGetTextChannelsAction(new[] { textChannel }).Build();

        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(guild));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(user));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(guild, user));
        await Repository.AddAsync(Database.Entity.GuildChannel.FromDiscord(textChannel, ChannelType.Text));
        await Repository.AddAsync(new Database.Entity.SearchItem
        {
            ChannelId = Consts.ChannelId.ToString(),
            GuildId = Consts.GuildId.ToString(),
            MessageContent = "Content",
            UserId = Consts.UserId.ToString(),
            Id = 1L
        });
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_NoItems()
        => await Instance.ProcessAsync(new[] { 1L });

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        await InitDataAsync();
        await Instance.ProcessAsync(new[] { 1L });
    }
}
