using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions.Api.V1.Invite;
using GrillBot.Data.Exceptions;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Invite;

[TestClass]
public class DeleteInviteTests : ApiActionTest<DeleteInvite>
{
    protected override DeleteInvite CreateAction()
    {
        var texts = new TextsBuilder()
            .AddText("Invite/NotFound", "cs", "NotFound")
            .Build();

        return new DeleteInvite(ApiRequestContext, DatabaseBuilder, texts);
    }

    private async Task InitDataAsync()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var user = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var guildUser = Database.Entity.GuildUser.FromDiscord(guild, user);
        guildUser.UsedInviteCode = Consts.InviteCode;

        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(guild));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(user));
        await Repository.AddAsync(guildUser);
        await Repository.AddAsync(new Database.Entity.Invite
        {
            GuildId = Consts.GuildId.ToString(),
            Code = Consts.InviteCode,
            CreatedAt = DateTime.Now,
            CreatorId = Consts.UserId.ToString()
        });
        await Repository.CommitAsync();
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(NotFoundException))]
    public async Task ProcessAsync_InviteNotFound()
    {
        await Action.ProcessAsync(Consts.GuildId, Consts.InviteCode);
    }

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        await InitDataAsync();
        await Action.ProcessAsync(Consts.GuildId, Consts.InviteCode);
    }
}
