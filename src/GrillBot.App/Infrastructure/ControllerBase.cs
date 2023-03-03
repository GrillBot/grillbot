using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Infrastructure;

[ExcludeFromCodeCoverage]
public abstract class ControllerBase : Controller
{
    protected IServiceProvider ServiceProvider { get; }

    protected ControllerBase(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    protected async Task<TData> ProcessActionAsync<TAction, TData>(Func<TAction, Task<TData>> asyncExecution) where TAction : notnull
    {
        var action = ServiceProvider.GetRequiredService<TAction>();
        return await asyncExecution(action);
    }

    protected async Task ProcessActionAsync<TAction>(Func<TAction, Task> asyncExecution) where TAction : notnull
        => await asyncExecution(ServiceProvider.GetRequiredService<TAction>());

    protected TData ProcessAction<TAction, TData>(Func<TAction, TData> syncExecution) where TAction : notnull
    {
        var action = ServiceProvider.GetRequiredService<TAction>();
        return syncExecution(action);
    }

    protected void ProcessAction<TAction>(Action<TAction> syncExecution) where TAction : notnull
        => syncExecution(ServiceProvider.GetRequiredService<TAction>());
}
