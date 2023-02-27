using System.Diagnostics.CodeAnalysis;
using Discord;
using GrillBot.App.Actions.Api.V1.User;
using GrillBot.App.Managers;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Data.Models.Unverify;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Common.Attributes;
using GrillBot.Tests.Infrastructure.Discord;
using Newtonsoft.Json;

namespace GrillBot.Tests.App.Actions.Api.V1.User;

[TestClass]
public class GetUserDetailTests : ApiActionTest<GetUserDetail>
{
    private IGuildUser User { get; set; } = null!;
    private IGuild Guild { get; set; } = null!;
    private ITextChannel TextChannel { get; set; } = null!;

    protected override GetUserDetail CreateInstance()
    {
        var role = new RoleBuilder(Consts.RoleId, Consts.RoleName).Build();
        var guildBuilder = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetRoles(new[] { role });
        User = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guildBuilder.Build()).SetRoles(new[] { role.Id }).SetActiveDevices(new[] { ClientType.Desktop })
            .Build();
        TextChannel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(guildBuilder.Build()).Build();
        Guild = guildBuilder.SetGetUsersAction(new[] { User }).SetGetTextChannelsAction(new[] { TextChannel }).Build();

        var client = new ClientBuilder()
            .SetGetUserAction(User)
            .SetGetGuildsAction(new[] { Guild })
            .Build();

        return new GetUserDetail(ApiRequestContext, DatabaseBuilder, TestServices.AutoMapper.Value, client, TestServices.Texts.Value);
    }

    private async Task InitDataAsync(ulong userId, bool withUnverify = true)
    {
        var entity = Database.Entity.User.FromDiscord(User);
        entity.Id = userId.ToString();
        await Repository.AddAsync(entity);

        var guildUser = Database.Entity.GuildUser.FromDiscord(Guild, User);
        guildUser.UsedInviteCode = "ABCD";

        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.AddAsync(guildUser);
        await Repository.AddAsync(Database.Entity.GuildChannel.FromDiscord(TextChannel, ChannelType.Text));
        await Repository.AddAsync(new Database.Entity.GuildUserChannel
        {
            Count = 50,
            ChannelId = Consts.ChannelId.ToString(),
            GuildId = Consts.GuildId.ToString(),
            UserId = Consts.UserId.ToString(),
            FirstMessageAt = DateTime.MinValue,
            LastMessageAt = DateTime.MaxValue
        });
        await Repository.AddAsync(new Database.Entity.Invite
        {
            Code = "ABCD",
            CreatedAt = DateTime.Now,
            CreatorId = Consts.UserId.ToString(),
            GuildId = Consts.GuildId.ToString()
        });
        await Repository.AddAsync(new Database.Entity.EmoteStatisticItem
        {
            EmoteId = Consts.FeelsHighManEmote,
            FirstOccurence = DateTime.Now,
            GuildId = Consts.GuildId.ToString(),
            LastOccurence = DateTime.Now,
            UseCount = 50,
            UserId = Consts.UserId.ToString()
        });
        if (withUnverify)
        {
            await Repository.AddAsync(new Database.Entity.Unverify
            {
                Channels = new List<Database.Entity.GuildChannelOverride>(),
                Reason = "Reason",
                Roles = new List<string>(),
                EndAt = DateTime.Now,
                GuildId = Consts.GuildId.ToString(),
                StartAt = DateTime.Now,
                UnverifyLog = new Database.Entity.UnverifyLog
                {
                    Data = JsonConvert.SerializeObject(new UnverifyLogSet
                    {
                        End = DateTime.Now,
                        Reason = "Reason",
                        Start = DateTime.Now,
                        ChannelsToKeep = new List<ChannelOverride>(),
                        ChannelsToRemove = new List<ChannelOverride>(),
                        IsSelfUnverify = false,
                        RolesToKeep = new List<ulong>(),
                        RolesToRemove = new List<ulong>()
                    }),
                    Operation = UnverifyOperation.Unverify,
                    CreatedAt = DateTime.Now,
                    GuildId = Consts.GuildId.ToString(),
                    FromUserId = Consts.UserId.ToString(),
                    ToUserId = Consts.UserId.ToString()
                },
                UserId = Consts.UserId.ToString()
            });
        }

        await Repository.AddAsync(new Database.Entity.AuditLogItem
        {
            Data = JsonConvert.SerializeObject(new MemberUpdatedData
            {
                Nickname = new Diff<string>("A", "B"),
                Target = new AuditUserInfo(User)
            }, AuditLogWriteManager.SerializerSettings),
            Type = AuditLogItemType.MemberUpdated,
            CreatedAt = DateTime.Now,
            GuildId = Consts.GuildId.ToString(),
            ProcessedUserId = Consts.UserId.ToString()
        });

        await Repository.CommitAsync();
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_NotFound()
        => await Instance.ProcessAsync(Consts.UserId);

    [TestMethod]
    public async Task ProcessAsync_Success_NotFoundOnDiscord()
    {
        await InitDataAsync(Consts.UserId + 1);

        var result = await Instance.ProcessAsync(Consts.UserId + 1);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        await InitDataAsync(Consts.UserId);

        var result = await Instance.ProcessAsync(Consts.UserId);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Guilds.Count > 0);
    }

    [TestMethod]
    [ApiConfiguration(true)]
    public async Task ProcessAsync_Self()
    {
        await InitDataAsync(Consts.UserId, false);

        var result = await Instance.ProcessSelfAsync();

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Guilds.Count > 0);
    }
}
