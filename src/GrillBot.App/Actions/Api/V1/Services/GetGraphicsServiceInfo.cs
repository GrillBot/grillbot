using GrillBot.Common.Managers.Logging;
using GrillBot.Common.Models;
using GrillBot.Common.Services.Graphics;
using GrillBot.Data.Models.API.Services;

namespace GrillBot.App.Actions.Api.V1.Services;

public class GetGraphicsServiceInfo : ApiAction
{
    private IGraphicsClient GraphicsClient { get; }
    private LoggingManager LoggingManager { get; }

    public GetGraphicsServiceInfo(ApiRequestContext apiContext, IGraphicsClient graphicsClient, LoggingManager loggingManager) : base(apiContext)
    {
        GraphicsClient = graphicsClient;
        LoggingManager = loggingManager;
    }

    public async Task<GraphicsServiceInfo> ProcessAsync()
    {
        var info = new GraphicsServiceInfo
        {
            Timeout = GraphicsClient.Timeout,
            Url = GraphicsClient.Url
        };

        try
        {
            info.Version = await GraphicsClient.GetVersionAsync();
            info.Metrics = await GraphicsClient.GetMetricsAsync();
            info.Statistics = await GraphicsClient.GetStatisticsAsync();
        }
        catch (Exception ex)
        {
            await LoggingManager.ErrorAsync("API", "An error occured while loading diagnostics info from Graphics service.", ex);
            info.ApiErrorMessage = ex.Message;
        }

        return info;
    }
}
