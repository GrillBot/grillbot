using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Discord;
using GrillBot.App.Actions.Api.V1.Channel;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Channel;

[TestClass]
public class GetChannelSimpleListTests : ApiActionTest<GetChannelSimpleList>
{
    private IGuild Guild { get; set; }
    private IGuildUser User { get; set; }
    private ITextChannel TextChannel { get; set; }
    private ITextChannel AnotherChannel { get; set; }

    protected override GetChannelSimpleList CreateAction()
    {
        var guildBuilder = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.Username);

        User = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guildBuilder.Build()).Build();
        TextChannel = new TextChannelBuilder().SetIdentity(Consts.ChannelId, Consts.ChannelName).SetGuild(guildBuilder.Build()).SetGetUsersAction(new[] { User }).Build();
        Guild = guildBuilder.SetGetUsersAction(new[] { User }).SetGetTextChannelsAction(new[] { TextChannel }).Build();
        AnotherChannel = new TextChannelBuilder().SetIdentity(Consts.ChannelId + 1, Consts.ChannelName).SetGuild(Guild).Build();

        var client = new ClientBuilder()
            .SetGetGuildsAction(new[] { Guild })
            .Build();
        var texts = new TextsBuilder()
            .Build();

        return new GetChannelSimpleList(ApiRequestContext, client, TestServices.AutoMapper.Value, DatabaseBuilder, texts);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    [ControllerTestConfiguration(true)]
    public async Task ProcessAsync_NoMutualGuild()
    {
        await Action.ProcessAsync(Consts.GuildId + 1, false);
    }

    [TestMethod]
    [ControllerTestConfiguration(true)]
    public async Task ProcessAsync_Success_Public()
    {
        var result = await Action.ProcessAsync(null, false);
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task ProcessAsync_Success_Private()
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(Guild, User));
        await Repository.AddAsync(Database.Entity.GuildChannel.FromDiscord(TextChannel, ChannelType.Text));
        await Repository.AddAsync(Database.Entity.GuildChannel.FromDiscord(AnotherChannel, ChannelType.Text));
        await Repository.CommitAsync();

        var result = await Action.ProcessAsync(null, false);
        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task ProcessAsync_Success_NoThreads()
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(Guild, User));
        await Repository.AddAsync(Database.Entity.GuildChannel.FromDiscord(TextChannel, ChannelType.Text));
        await Repository.CommitAsync();

        var result = await Action.ProcessAsync(null, true);
        Assert.AreEqual(1, result.Count);
    }
}
