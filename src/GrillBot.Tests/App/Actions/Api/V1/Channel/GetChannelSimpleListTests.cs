using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Discord;
using GrillBot.App.Actions.Api.V1.Channel;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Common.Attributes;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Channel;

[TestClass]
public class GetChannelSimpleListTests : ApiActionTest<GetChannelSimpleList>
{
    private IGuild Guild { get; set; }
    private IGuildUser User { get; set; }
    private ITextChannel TextChannel { get; set; }
    private ITextChannel AnotherChannel { get; set; }

    protected override GetChannelSimpleList CreateInstance()
    {
        var guildBuilder = new GuildBuilder(Consts.GuildId, Consts.Username);

        User = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guildBuilder.Build()).Build();
        TextChannel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(guildBuilder.Build()).SetGetUsersAction(new[] { User }).Build();
        Guild = guildBuilder.SetGetUsersAction(new[] { User }).SetGetTextChannelsAction(new[] { TextChannel }).Build();
        AnotherChannel = new TextChannelBuilder(Consts.ChannelId + 1, Consts.ChannelName).SetGuild(Guild).Build();

        var client = new ClientBuilder()
            .SetGetGuildsAction(new[] { Guild })
            .Build();
        return new GetChannelSimpleList(ApiRequestContext, client, TestServices.AutoMapper.Value, DatabaseBuilder, TestServices.Texts.Value);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    [ApiConfiguration(true)]
    public async Task ProcessAsync_NoMutualGuild()
    {
        await Instance.ProcessAsync(Consts.GuildId + 1, false);
    }

    [TestMethod]
    [ApiConfiguration(true)]
    public async Task ProcessAsync_Success_Public()
    {
        var result = await Instance.ProcessAsync(null, false);
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

        var result = await Instance.ProcessAsync(null, false);
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

        var result = await Instance.ProcessAsync(null, true);
        Assert.AreEqual(1, result.Count);
    }
}
