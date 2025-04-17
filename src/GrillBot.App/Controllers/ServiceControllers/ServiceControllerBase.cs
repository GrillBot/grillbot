using GrillBot.App.Actions;
using GrillBot.App.Actions.Api;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.Common.Executor;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers.ServiceControllers;

[ApiExplorerSettings(GroupName = "v3")]
[Route("api/service/[controller]")]
public abstract class ServiceControllerBase<TService>(
    IServiceProvider serviceProvider
) : Core.Infrastructure.Actions.ControllerBase(serviceProvider) where TService : IServiceClient
{
    protected Task<IActionResult> ExecuteAsync(Func<TService, ServiceExecutorContext, Task<object>> executor, IDictionaryObject? parameters = null)
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

    protected Task<IActionResult> ExecuteAsync(Func<TService, ServiceExecutorContext, Task> executor, IDictionaryObject? parameters = null)
    {
        if (parameters is not null)
            ApiAction.Init(this, parameters);

        return ProcessAsync<ServiceBridgeAction<TService>>(executor);
    }

    protected Task<IActionResult> ExecuteRabbitPayloadAsync(Func<object> createPayload)
        => ProcessAsync<RabbitMQPublisherAction>(createPayload());
}
