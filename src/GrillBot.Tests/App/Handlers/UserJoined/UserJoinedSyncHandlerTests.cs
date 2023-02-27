using Discord;
using GrillBot.App.Handlers.UserJoined;
using GrillBot.App.Managers;
using GrillBot.Data.Models;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;
using Newtonsoft.Json;

namespace GrillBot.Tests.App.Handlers.UserJoined;

[TestClass]
public class UserJoinedSyncHandlerTests : TestBase<UserJoinedSyncHandler>
{
    private IGuildUser User { get; set; } = null!;
    private IGuildUser InitializedUser { get; set; } = null!;

    protected override void PreInit()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        User = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        InitializedUser = new GuildUserBuilder(Consts.UserId + 1, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
    }

    protected override UserJoinedSyncHandler CreateInstance()
    {
        return new UserJoinedSyncHandler(DatabaseBuilder);
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(User.Guild, User));
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(User.Guild));

        var initializedUser = Database.Entity.User.FromDiscord(InitializedUser);
        initializedUser.Language = "cs";
        await Repository.AddAsync(initializedUser);

        var initialized = Database.Entity.GuildUser.FromDiscord(User.Guild, InitializedUser);
        initialized.Nicknames.Add(new Database.Entity.Nickname { Id = 1, NicknameValue = "Nickname" });
        await Repository.AddAsync(initialized);

        await Repository.AddCollectionAsync(new[]
        {
            new Database.Entity.AuditLogItem
            {
                Type = AuditLogItemType.InteractionCommand,
                ProcessedUserId = User.Id.ToString(),
                GuildId = User.GuildId.ToString(),
                CreatedAt = DateTime.Now,
                Data = JsonConvert.SerializeObject(new GrillBot.Data.Models.AuditLog.InteractionCommandExecuted
                {
                    Locale = "cs"
                }, AuditLogWriteManager.SerializerSettings)
            },
            new Database.Entity.AuditLogItem
            {
                Type = AuditLogItemType.MemberUpdated,
                CreatedAt = DateTime.Now,
                GuildId = User.GuildId.ToString(),
                ProcessedUserId = User.Id.ToString(),
                Data = JsonConvert.SerializeObject(new MemberUpdatedData
                {
                    Nickname = new Diff<string>
                    {
                        After = "After",
                        Before = "Before"
                    },
                    Target = new AuditUserInfo(User)
                }, AuditLogWriteManager.SerializerSettings)
            }
        });
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_NoDb() 
        => await Instance.ProcessAsync(User);

    [TestMethod]
    public async Task ProcessAsync_WithDb()
    {
        await InitDataAsync();
        await Instance.ProcessAsync(User);
    }

    [TestMethod]
    public async Task ProcessAsync_FullyInitialized()
    {
        await InitDataAsync();
        await Instance.ProcessAsync(InitializedUser);
    }
}
