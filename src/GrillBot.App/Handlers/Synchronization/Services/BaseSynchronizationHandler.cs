using GrillBot.Core.Services.Common;

namespace GrillBot.App.Handlers.Synchronization.Services;

public class BaseSynchronizationHandler<TServiceClient> : BaseSynchronizationHandler where TServiceClient : IClient
{
    protected TServiceClient ServiceClient { get; }

    protected BaseSynchronizationHandler(TServiceClient serviceClient, GrillBotDatabaseBuilder databaseBuilder) : base(databaseBuilder)
    {
        ServiceClient = serviceClient;
    }
}
