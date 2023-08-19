using GrillBot.Common.Models;
using GrillBot.Core.Services.Common;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Actions.Api;

public class ApiBridgeAction : ApiAction
{
    private IServiceProvider ServiceProvider { get; }

    public ApiBridgeAction(ApiRequestContext apiContext, IServiceProvider serviceProvider) : base(apiContext)
    {
        ServiceProvider = serviceProvider;
    }

    public async Task<TResult> ExecuteAsync<TServiceClient, TResult>(Func<TServiceClient, Task<TResult>> asyncExecutor) where TServiceClient : IClient 
        => await asyncExecutor(ServiceProvider.GetRequiredService<TServiceClient>());

    public async Task ExecuteAsync<TServiceClient>(Func<TServiceClient, Task> asyncExecutor) where TServiceClient : IClient
        => await asyncExecutor(ServiceProvider.GetRequiredService<TServiceClient>());
}
