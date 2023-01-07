using System.Diagnostics.CodeAnalysis;
using Discord;
using GrillBot.App.Actions.Api.V1.Guild;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Guild;

[TestClass]
public class GetGuildDetailTests : ApiActionTest<GetGuildDetail>
{
    private IGuild Guild { get; set; }
    private ITextChannel TextChannel { get; set; }
    private IGuildUser User { get; set; }
    private IRole Role { get; set; }

    protected override GetGuildDetail CreateAction()
    {
        var guildBuilder = new GuildBuilder(Consts.GuildId, Consts.GuildName);
        TextChannel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(guildBuilder.Build()).Build();
        User = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guildBuilder.Build()).SetStatus(UserStatus.Online).Build();
        Role = new RoleBuilder(Consts.RoleId, Consts.RoleName).Build();
        Guild = guildBuilder.SetGetChannelsAction(new[] { TextChannel }).SetGetUsersAction(new[] { User }).SetRoles(new[] { Role }).Build();

        var client = new ClientBuilder()
            .SetGetGuildsAction(new[] { Guild })
            .Build();
        return new GetGuildDetail(ApiRequestContext, DatabaseBuilder, TestServices.AutoMapper.Value, client, CacheBuilder, TestServices.Texts.Value);
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_GuildNotFound()
    {
        await Action.ProcessAsync(Consts.GuildId);
    }

    [TestMethod]
    public async Task ProcessAsync_WithoutDiscordGuild()
    {
        var anotherGuild = new GuildBuilder(Consts.GuildId + 1, Consts.GuildName).Build();
        await InitGuildAsync(anotherGuild, false);

        var result = await Action.ProcessAsync(anotherGuild.Id);

        Assert.IsNotNull(result);
        Assert.AreEqual(result.Name, anotherGuild.Name);
    }

    [TestMethod]
    public async Task ProcessAsync_WithFullDetails()
    {
        await InitGuildAsync(Guild, true);

        var result = await Action.ProcessAsync(Guild.Id);

        CheckSuccess(result, true);
        Assert.AreEqual(Guild.Name, result.Name);
        Assert.AreEqual(Guild.Id.ToString(), result.Id);
    }

    public static void CheckSuccess(GuildDetail result, bool full)
    {
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.AdminChannel);
        Assert.IsNotNull(result.EmoteSuggestionChannel);
        Assert.IsNotNull(result.MutedRole);
        Assert.IsNotNull(result.VoteChannel);

        if (!full)
            return;

        Assert.IsNotNull(result.BoosterRole);
        Assert.IsTrue(result.UserStatusReport.Count > 0);
        Assert.IsTrue(result.ClientTypeReport.Count > 0);
    }

    private async Task InitGuildAsync(IGuild guild, bool full)
    {
        var entity = Database.Entity.Guild.FromDiscord(guild);

        if (full)
        {
            entity.AdminChannelId = Consts.ChannelId.ToString();
            entity.BoosterRoleId = Consts.RoleId.ToString();
            entity.MuteRoleId = Consts.RoleId.ToString();
            entity.VoteChannelId = Consts.ChannelId.ToString();
            entity.EmoteSuggestionChannelId = Consts.ChannelId.ToString();
        }

        await Repository.AddAsync(entity);
        await Repository.CommitAsync();
    }
}
