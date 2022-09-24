using GrillBot.App.Actions.Api.V1.Command;
using GrillBot.Data.Models.API.Permissions;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.Command;

[TestClass]
public class CreateExplicitPermissionTests : ApiActionTest<CreateExplicitPermission>
{
    protected override CreateExplicitPermission CreateAction()
    {
        var texts = new TextsBuilder()
            .AddText("ExplicitPerms/Create/Conflict", "cs", "Conflict")
            .Build();

        return new CreateExplicitPermission(ApiRequestContext, DatabaseBuilder, texts);
    }

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        var parameters = new CreateExplicitPermissionParams
        {
            Command = "$unverify",
            IsRole = false,
            State = Database.Enums.ExplicitPermissionState.Allowed,
            TargetId = Consts.UserId.ToString()
        };

        await Action.ProcessAsync(parameters);

        Assert.IsFalse(Action.IsConflict);
        Assert.IsTrue(string.IsNullOrEmpty(Action.ErrorMessage));
    }

    [TestMethod]
    public async Task ProcessAsync_Conflict()
    {
        var parameters = new CreateExplicitPermissionParams
        {
            Command = "unverify",
            IsRole = false,
            State = Database.Enums.ExplicitPermissionState.Allowed,
            TargetId = Consts.UserId.ToString()
        };

        await Action.ProcessAsync(parameters);
        Assert.IsFalse(Action.IsConflict);

        await Action.ProcessAsync(parameters);
        Assert.IsTrue(Action.IsConflict);
    }
}
