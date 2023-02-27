using System.Diagnostics.CodeAnalysis;
using GrillBot.Common.Services.KachnaOnline.Models;
using GrillBot.Data.Exceptions;

namespace GrillBot.Tests.App.Actions.Commands.DuckInfoTests;

[TestClass]
public class HttpException : DuckInfoTestsBase
{
    protected override DuckState? State => null;

    [TestMethod]
    [ExpectedException(typeof(GrillBotException))]
    [ExcludeFromCodeCoverage]
    public override Task RunTestAsync() => Instance.ProcessAsync();
}
