using GrillBot.App.Actions;
using GrillBot.App.Actions.Api;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.Common;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers.ServiceControllers;

[ApiExplorerSettings(GroupName = "v3")]
[Route("api/service/[controller]")]
public abstract class ServiceControllerBase<TService> : Core.Infrastructure.Actions.ControllerBase where TService : IClient
{
    protected ServiceControllerBase(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected Task<IActionResult> ExecuteAsync(Func<TService, Task<object>> executor, IDictionaryObject? parameters = null)
    {
        if (parameters is not null)
            ApiAction.Init(this, parameters);

        return ProcessAsync<ServiceBridgeAction<TService>>(executor);
    }

    protected Task<IActionResult> ExecuteAsync<TAction>(params object[] parameters) where TAction : ApiActionBase
    {
        foreach (var parameter in parameters.OfType<IDictionaryObject>())
            ApiAction.Init(this, parameter);

        return ProcessAsync<TAction>(parameters);
    }

    protected Task<IActionResult> ExecuteAsync(Func<TService, Task> executor, IDictionaryObject? parameters = null)
    {
        if (parameters is not null)
            ApiAction.Init(this, parameters);

        return ProcessAsync<ServiceBridgeAction<TService>>(executor);
    }

    protected Task<IActionResult> ExecuteRabbitPayloadAsync(Func<object> createPayload)
        => ProcessAsync<RabbitMQPublisherAction>(createPayload());
}
