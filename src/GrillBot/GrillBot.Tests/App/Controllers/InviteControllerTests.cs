using GrillBot.App.Controllers;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Invites;
using GrillBot.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class InviteControllerTests : ControllerTest<InviteController>
{
    protected override InviteController CreateController()
    {
        var dbFactory = new DbContextBuilder();
        DbContext = dbFactory.Create();

        return new InviteController(DbContext);
    }

    public override void Cleanup()
    {
        DbContext.Invites.RemoveRange(DbContext.Invites.AsEnumerable());
        DbContext.GuildUsers.RemoveRange(DbContext.GuildUsers.AsEnumerable());
        DbContext.Users.RemoveRange(DbContext.Users.AsEnumerable());
        DbContext.Guilds.RemoveRange(DbContext.Guilds.AsEnumerable());
        DbContext.SaveChanges();
    }

    [TestMethod]
    public async Task GetInviteListAsync_WithoutFilter()
    {
        await DbContext.Invites.AddAsync(new Database.Entity.Invite() { Code = "Code", CreatorId = "12345", GuildId = "12345" });
        await DbContext.Guilds.AddAsync(new Database.Entity.Guild() { Name = "Guild", Id = "12345" });
        await DbContext.GuildUsers.AddAsync(new Database.Entity.GuildUser() { GuildId = "12345", UserId = "12345" });
        await DbContext.Users.AddAsync(new Database.Entity.User() { Username = "Username", Discriminator = "1234", Id = "12345" });
        await DbContext.SaveChangesAsync();

        var result = await Controller.GetInviteListAsync(new GetInviteListParams(), CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<GuildInvite>>(result);
    }

    [TestMethod]
    public async Task GetInviteListAsync_WithFilter()
    {
        var filter = new GetInviteListParams()
        {
            Code = "Code",
            CreatedFrom = System.DateTime.MinValue,
            CreatedTo = System.DateTime.MaxValue,
            CreatorId = "12345",
            GuildId = "12345"
        };

        var result = await Controller.GetInviteListAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<GuildInvite>>(result);
    }
}
