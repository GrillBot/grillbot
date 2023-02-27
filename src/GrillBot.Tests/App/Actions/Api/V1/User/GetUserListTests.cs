using System.Linq;
using Discord;
using GrillBot.App.Actions.Api.V1.User;
using GrillBot.Data.Models.API.Users;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.User;

[TestClass]
public class GetUserListTests : ApiActionTest<GetUserList>
{
    private IGuild[] Guilds { get; set; }
    private IGuildUser User { get; set; }

    protected override GetUserList CreateInstance()
    {
        var guildBuilder = new GuildBuilder(Consts.GuildId, Consts.GuildName);
        User = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guildBuilder.Build()).Build();

        Guilds = new[]
        {
            guildBuilder.SetGetUsersAction(new[] { User }).Build(),
            new GuildBuilder(Consts.GuildId + 1, Consts.GuildName + "2").Build(),
            new GuildBuilder(Consts.GuildId + 2, Consts.GuildName + "3").Build()
        };

        var client = new ClientBuilder()
            .SetGetGuildsAction(Guilds.Take(2).ToArray())
            .Build();

        return new GetUserList(ApiRequestContext, DatabaseBuilder, TestServices.AutoMapper.Value, client);
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));

        foreach (var guild in Guilds)
        {
            await Repository.AddAsync(Database.Entity.Guild.FromDiscord(guild));
            await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(guild, User));
        }

        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_WithoutFilter()
    {
        await InitDataAsync();

        var filter = new GetUserListParams { Sort = { Descending = true } };
        var result = await Instance.ProcessAsync(filter);

        Assert.AreEqual(1, result.TotalItemsCount);
        Assert.AreEqual(3, result.Data[0].Guilds.Count);
    }

    [TestMethod]
    public async Task ProcessAsync_WithFilter()
    {
        var filter = new GetUserListParams
        {
            Flags = long.MaxValue,
            Status = UserStatus.Online,
            Username = "Username",
            GuildId = Consts.GuildId.ToString(),
            HaveBirthday = true,
            UsedInviteCode = "ABCDEFGH",
            Sort = { Descending = false }
        };

        var result = await Instance.ProcessAsync(filter);
        Assert.AreEqual(0, result.TotalItemsCount);
    }
}
