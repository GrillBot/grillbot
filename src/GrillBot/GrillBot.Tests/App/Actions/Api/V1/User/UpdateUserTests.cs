using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions.Api.V1.User;
using GrillBot.App.Managers;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API.Users;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.User;

[TestClass]
public class UpdateUserTests : ApiActionTest<UpdateUser>
{
    protected override UpdateUser CreateAction()
    {
        var texts = new TextsBuilder().AddText("User/NotFound", "cs", "NotFound").Build();
        var auditLogWriter = new AuditLogWriteManager(DatabaseBuilder);
        return new UpdateUser(ApiRequestContext, DatabaseBuilder, auditLogWriter, texts);
    }

    private async Task InitDataAsync()
    {
        var user = new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

        await Repository.AddAsync(Database.Entity.User.FromDiscord(user));
        await Repository.CommitAsync();
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_NotFound()
        => await Action.ProcessAsync(Consts.UserId, new UpdateUserParams());

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        await InitDataAsync();

        var parameters = new UpdateUserParams
        {
            Note = "Note",
            BotAdmin = true,
            CommandsDisabled = true,
            PointsDisabled = true,
            PublicAdminBlocked = true,
            WebAdminAllowed = true,
            SelfUnverifyMinimalTime = TimeSpan.MaxValue
        };

        await Action.ProcessAsync(Consts.UserId, parameters);
    }
}
