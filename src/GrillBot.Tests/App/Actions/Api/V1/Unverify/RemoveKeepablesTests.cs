using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions.Api.V1.Unverify;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.Unverify;

[TestClass]
public class RemoveKeepablesTests : ApiActionTest<RemoveKeepables>
{
    protected override RemoveKeepables CreateInstance()
    {
        return new RemoveKeepables(ApiRequestContext, DatabaseBuilder, TestServices.Texts.Value);
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(new Database.Entity.SelfunverifyKeepable { GroupName = "1bit", Name = "izp" });
        await Repository.CommitAsync();
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_ItemNotFound()
        => await Instance.ProcessAsync("1bit", "izp");

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_GroupNotFound()
        => await Instance.ProcessAsync("1bit");

    [TestMethod]
    public async Task ProcessAsync_Success_Group()
    {
        await InitDataAsync();
        await Instance.ProcessAsync("1bit");
    }

    [TestMethod]
    public async Task ProcessAsync_Success_Item()
    {
        await InitDataAsync();
        await Instance.ProcessAsync("1bit", "izp");
    }
}
