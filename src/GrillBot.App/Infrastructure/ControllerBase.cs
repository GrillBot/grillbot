using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Infrastructure;

public abstract class ControllerBase : Core.Infrastructure.Actions.ControllerBase
{
    protected ControllerBase(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected async Task<TData> ProcessActionAsync<TAction, TData>(Func<TAction, Task<TData>> asyncExecution) where TAction : notnull
        => await asyncExecution(ServiceProvider.GetRequiredService<TAction>());

    protected async Task ProcessActionAsync<TAction>(Func<TAction, Task> asyncExecution) where TAction : notnull
        => await asyncExecution(ServiceProvider.GetRequiredService<TAction>());

    protected TData ProcessAction<TAction, TData>(Func<TAction, TData> syncExecution) where TAction : notnull
        => syncExecution(ServiceProvider.GetRequiredService<TAction>());
}
