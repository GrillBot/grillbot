using Discord;
using GrillBot.App.Actions.Api.V1.Unverify;
using GrillBot.Data.Models;
using GrillBot.Data.Models.API.Unverify;
using GrillBot.Database.Enums;
using GrillBot.Database.Models;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Common.Attributes;
using GrillBot.Tests.Infrastructure.Discord;
using Newtonsoft.Json;
using UnverifyLogRemove = GrillBot.Data.Models.Unverify.UnverifyLogRemove;
using UnverifyLogSet = GrillBot.Data.Models.Unverify.UnverifyLogSet;
using UnverifyLogUpdate = GrillBot.Data.Models.Unverify.UnverifyLogUpdate;

namespace GrillBot.Tests.App.Actions.Api.V1.Unverify;

[TestClass]
public class GetLogsTests : ApiActionTest<GetLogs>
{
    private IGuild Guild { get; set; }
    private IGuildUser User { get; set; }

    protected override GetLogs CreateAction()
    {
        var guildBuilder = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName);
        User = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guildBuilder.Build()).Build();
        Guild = guildBuilder.SetGetUserAction(User).Build();

        var client = new ClientBuilder()
            .SetGetGuildsAction(new[] { Guild })
            .SetGetUserAction(User)
            .Build();

        return new GetLogs(ApiRequestContext, client, TestServices.AutoMapper.Value, DatabaseBuilder);
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(Guild, User));

        var logItems = new (UnverifyOperation, object)[]
        {
            (UnverifyOperation.Selfunverify, new UnverifyLogSet()),
            (UnverifyOperation.Autoremove, new UnverifyLogRemove { ReturnedOverwrites = new List<ChannelOverride> { new() }, ReturnedRoles = new List<ulong> { 0 } }),
            (UnverifyOperation.Recover, new UnverifyLogRemove()),
            (UnverifyOperation.Remove, new UnverifyLogRemove()),
            (UnverifyOperation.Unverify, new UnverifyLogSet
            {
                ChannelsToKeep = new List<ChannelOverride> { new() }, ChannelsToRemove = new List<ChannelOverride> { new() }, RolesToKeep = new List<ulong> { 0 },
                RolesToRemove = new List<ulong> { 0 }
            }),
            (UnverifyOperation.Update, new UnverifyLogUpdate())
        };

        foreach (var item in logItems)
        {
            await Repository.AddAsync(new Database.Entity.UnverifyLog
            {
                Data = JsonConvert.SerializeObject(item.Item2),
                Operation = item.Item1,
                CreatedAt = DateTime.Now,
                GuildId = Consts.GuildId.ToString(),
                FromUserId = Consts.UserId.ToString(),
                ToUserId = Consts.UserId.ToString()
            });
        }

        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_Private_WithoutFilter()
    {
        await InitDataAsync();

        var filter = new UnverifyLogParams();
        var result = await Action.ProcessAsync(filter);

        Assert.AreEqual(Enum.GetValues<UnverifyOperation>().Length, result.TotalItemsCount);
    }

    [TestMethod]
    public async Task ProcessAsync_WithFilter()
    {
        var filter = new UnverifyLogParams
        {
            Created = new RangeParams<DateTime?> { From = DateTime.MinValue, To = DateTime.MaxValue },
            Operation = UnverifyOperation.Autoremove,
            GuildId = Consts.GuildId.ToString(),
            FromUserId = Consts.UserId.ToString(),
            ToUserId = Consts.UserId.ToString()
        };

        var result = await Action.ProcessAsync(filter);
        Assert.AreEqual(0, result.TotalItemsCount);
    }

    [TestMethod]
    [ApiConfiguration(true)]
    public async Task ProcessAsync_Public()
    {
        await InitDataAsync();

        var filter = new UnverifyLogParams { GuildId = (Consts.GuildId + 1).ToString() };
        var result = await Action.ProcessAsync(filter);
        Assert.AreEqual(Enum.GetValues<UnverifyOperation>().Length, result.TotalItemsCount);
    }
}
