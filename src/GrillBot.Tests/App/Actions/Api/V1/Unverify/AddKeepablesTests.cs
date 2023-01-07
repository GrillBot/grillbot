using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions.Api.V1.Unverify;
using GrillBot.Data.Models.API.Selfunverify;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.Unverify;

[TestClass]
public class AddKeepablesTests : ApiActionTest<AddKeepables>
{
    protected override AddKeepables CreateAction()
    {
        return new AddKeepables(ApiRequestContext, DatabaseBuilder, TestServices.Texts.Value);
    }

    [TestMethod]
    public async Task ProcessAsync_NotExists()
    {
        var parameters = new List<KeepableParams>
        {
            new() { Group = "1BIT", Name = "IZP" },
            new() { Group = "2BIT", Name = "IAL" }
        };

        await Action.ProcessAsync(parameters);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_Exists()
    {
        await ProcessAsync_NotExists();
        await ProcessAsync_NotExists();
    }
}
