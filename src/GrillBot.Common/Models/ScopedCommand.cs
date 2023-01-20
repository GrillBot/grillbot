using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Common.Models;

/// <summary>
/// Wrapper for properly disposing loaded services from the container.
/// </summary>
public sealed class ScopedCommand<TCommand> : IDisposable where TCommand : notnull
{
    public TCommand Command { get; }
    private IServiceScope Scope { get; }

    public ScopedCommand(IServiceScope scope)
    {
        Scope = scope;
        Command = scope.ServiceProvider.GetRequiredService<TCommand>();
    }

    public TService Resolve<TService>() where TService : notnull
        => Scope.ServiceProvider.GetRequiredService<TService>();

    public void Dispose()
    {
        Scope.Dispose();
    }
}
