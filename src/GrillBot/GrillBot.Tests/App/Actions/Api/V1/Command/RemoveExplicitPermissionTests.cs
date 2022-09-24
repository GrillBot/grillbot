using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions.Api.V1.Command;
using GrillBot.Common.Managers.Localization;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API.Permissions;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.Command;

[TestClass]
public class RemoveExplicitPermissionTests : ApiActionTest<RemoveExplicitPermission>
{
    private ITextsManager Texts { get; set; }

    protected override RemoveExplicitPermission CreateAction()
    {
        Texts = new TextsBuilder()
            .AddText("ExplicitPerms/Remove/NotFound", "cs", "PermNotFound")
            .Build();

        return new RemoveExplicitPermission(ApiRequestContext, DatabaseBuilder, Texts);
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_NotFound()
    {
        await Action.ProcessAsync("unverify", Consts.UserId.ToString());
    }

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        var create = new CreateExplicitPermission(ApiRequestContext, DatabaseBuilder, Texts);
        await create.ProcessAsync(new CreateExplicitPermissionParams
        {
            Command = "unverify",
            State = ExplicitPermissionState.Allowed,
            IsRole = false,
            TargetId = Consts.UserId.ToString()
        });
        Assert.IsFalse(create.IsConflict);

        await Action.ProcessAsync("unverify", Consts.UserId.ToString());
    }
}
