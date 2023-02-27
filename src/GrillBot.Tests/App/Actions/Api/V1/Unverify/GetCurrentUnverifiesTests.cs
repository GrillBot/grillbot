using Discord;
using GrillBot.App.Actions.Api.V1.Unverify;
using GrillBot.Data.Models.Unverify;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Common.Attributes;
using GrillBot.Tests.Infrastructure.Discord;
using Newtonsoft.Json;

namespace GrillBot.Tests.App.Actions.Api.V1.Unverify;

[TestClass]
public class GetCurrentUnverifiesTests : ApiActionTest<GetCurrentUnverifies>
{
    private IGuild Guild { get; set; } = null!;
    private IGuildUser User { get; set; } = null!;

    protected override GetCurrentUnverifies CreateInstance()
    {
        Guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        User = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(Guild).Build();

        var client = new ClientBuilder()
            .SetGetGuildsAction(new[] { Guild })
            .SetGetUserAction(User)
            .Build();

        return new GetCurrentUnverifies(ApiRequestContext, TestServices.AutoMapper.Value, client, DatabaseBuilder);
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(Guild, User));

        await Repository.AddAsync(new Database.Entity.UnverifyLog
        {
            Data = JsonConvert.SerializeObject(new UnverifyLogSet()),
            Id = 1,
            Operation = UnverifyOperation.Selfunverify,
            Unverify = new Database.Entity.Unverify
            {
                GuildId = Consts.GuildId.ToString(),
                UserId = Consts.UserId.ToString(),
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddDays(1),
                SetOperationId = 1
            },
            CreatedAt = DateTime.Now,
            GuildId = Consts.GuildId.ToString(),
            FromUserId = Consts.UserId.ToString(),
            ToUserId = Consts.UserId.ToString()
        });
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_Private()
    {
        await InitDataAsync();

        var result = await Instance.ProcessAsync();
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    [ApiConfiguration(true)]
    public async Task ProcessAsync_Public()
    {
        await InitDataAsync();

        var result = await Instance.ProcessAsync();
        Assert.AreEqual(1, result.Count);
    }
}
