using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions.Api.V1.Command;
using GrillBot.Data.Exceptions;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.Command;

[TestClass]
public class SetExplicitPermissionStateTests : ApiActionTest<SetExplicitPermissionState>
{
    protected override SetExplicitPermissionState CreateAction()
    {
        var texts = new TextsBuilder()
            .AddText("ExplicitPerms/NotFound", "cs", "NotFound")
            .Build();

        return new SetExplicitPermissionState(ApiRequestContext, DatabaseBuilder, texts);
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_NotFound()
    {
        await Action.ProcessAsync("unverify", Consts.UserId.ToString(), ExplicitPermissionState.Allowed);
    }

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        await Repository.AddAsync(new Database.Entity.ExplicitPermission
        {
            Command = "unverify",
            IsRole = false,
            State = ExplicitPermissionState.Banned,
            TargetId = Consts.UserId.ToString()
        });
        await Repository.CommitAsync();

        await Action.ProcessAsync("unverify", Consts.UserId.ToString(), ExplicitPermissionState.Allowed);
    }
}
