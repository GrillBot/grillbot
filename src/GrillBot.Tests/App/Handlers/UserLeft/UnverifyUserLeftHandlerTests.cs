using Discord;
using GrillBot.App.Handlers.UserLeft;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Handlers.UserLeft;

[TestClass]
public class UnverifyUserLeftHandlerTests : HandlerTest<UnverifyUserLeftHandler>
{
    private IGuild Guild { get; set; } = null!;
    private IGuildUser User { get; set; } = null!;

    protected override UnverifyUserLeftHandler CreateHandler()
    {
        Guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        User = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(Guild).Build();

        return new UnverifyUserLeftHandler(DatabaseBuilder);
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(Guild, User));
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.AddAsync(new Database.Entity.Unverify
        {
            GuildId = Consts.GuildId.ToString(),
            UserId = Consts.UserId.ToString(),
            Reason = "Reason",
            Roles = new List<string>(),
            Channels = new List<Database.Entity.GuildChannelOverride>(),
            EndAt = DateTime.Now,
            StartAt = DateTime.Now
        });
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_NoUser()
        => await Handler.ProcessAsync(Guild, User);

    [TestMethod]
    public async Task ProcessAsync_WithUser()
    {
        await InitDataAsync();
        await Handler.ProcessAsync(Guild, User);
    }
}
