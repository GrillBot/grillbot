using GrillBot.App.Actions.Api;
using GrillBot.Core.Services.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Infrastructure;

public abstract class ControllerBase : Controller
{
    protected IServiceProvider ServiceProvider { get; }

    protected ControllerBase(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    protected async Task<TData> ProcessActionAsync<TAction, TData>(Func<TAction, Task<TData>> asyncExecution) where TAction : notnull
        => await asyncExecution(ServiceProvider.GetRequiredService<TAction>());

    protected async Task ProcessActionAsync<TAction>(Func<TAction, Task> asyncExecution) where TAction : notnull
        => await asyncExecution(ServiceProvider.GetRequiredService<TAction>());

    protected TData ProcessAction<TAction, TData>(Func<TAction, TData> syncExecution) where TAction : notnull
        => syncExecution(ServiceProvider.GetRequiredService<TAction>());

    protected void ProcessAction<TAction>(Action<TAction> syncExecution) where TAction : notnull
        => syncExecution(ServiceProvider.GetRequiredService<TAction>());

    protected Task ProcessBridgeAsync<TService>(Func<TService, Task> asyncExecutor) where TService : IClient
        => ProcessActionAsync<ApiBridgeAction>(bridge => bridge.ExecuteAsync(asyncExecutor));

    protected Task<TResult> ProcessBridgeAsync<TService, TResult>(Func<TService, Task<TResult>> asyncExecutor) where TService : IClient
        => ProcessActionAsync<ApiBridgeAction, TResult>(bridge => bridge.ExecuteAsync(asyncExecutor));
}
