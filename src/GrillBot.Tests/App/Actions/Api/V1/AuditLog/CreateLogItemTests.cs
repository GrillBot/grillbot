using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions.Api.V1.AuditLog;
using GrillBot.App.Managers;
using GrillBot.Data.Models.API.AuditLog;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.AuditLog;

[TestClass]
public class CreateLogItemTests : ApiActionTest<CreateLogItem>
{
    protected override CreateLogItem CreateInstance()
    {
        var writer = new AuditLogWriteManager(DatabaseBuilder);
        return new CreateLogItem(ApiRequestContext, writer, TestServices.Texts.Value);
    }

    [TestMethod]
    public async Task ProcessAsync_Info() => await ProcessTestAsync(true, false, false, "Test");

    [TestMethod]
    public async Task ProcessAsync_Warning() => await ProcessTestAsync(false, true, false, "Test");

    [TestMethod]
    public async Task ProcessAsync_Error() => await ProcessTestAsync(false, false, true, "Test");

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_Validation_OneRequired() => await ProcessTestAsync(false, false, false, "Test");

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_Validation_MultipleSelected() => await ProcessTestAsync(true, true, true, "Test");

    private async Task ProcessTestAsync(bool info, bool warning, bool error, string content)
    {
        var request = new ClientLogItemRequest { Content = content, IsError = error, IsInfo = info, IsWarning = warning };
        await Instance.ProcessAsync(request);
    }
}
