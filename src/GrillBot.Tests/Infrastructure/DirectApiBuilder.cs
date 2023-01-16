using GrillBot.App.Services.DirectApi;
using GrillBot.Data.Models.DirectApi;
using Moq;

namespace GrillBot.Tests.Infrastructure;

public class DirectApiBuilder : BuilderBase<IDirectApiService>
{
    public DirectApiBuilder SetSendCommandAction(string service, string commandId, string? result)
    {
        Mock.Setup(o => o.SendCommandAsync(It.Is<string>(x => x == service), It.Is<DirectMessageCommand>(x => x.ToString() == commandId)))
            .ReturnsAsync(result!);
        return this;
    }
}
