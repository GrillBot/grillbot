using GrillBot.Common.Managers.Logging;
using GrillBot.Common.Models;
using GrillBot.Common.Services.RubbergodService;
using GrillBot.Data.Models.API.Services;

namespace GrillBot.App.Actions.Api.V1.Services;

public class GetRubbergodServiceInfo : ApiAction
{
    private IRubbergodServiceClient RubbergodServiceClient { get; }
    private LoggingManager LoggingManager { get; }

    public GetRubbergodServiceInfo(ApiRequestContext apiContext, IRubbergodServiceClient rubbergodServiceClient, LoggingManager loggingManager) : base(apiContext)
    {
        RubbergodServiceClient = rubbergodServiceClient;
        LoggingManager = loggingManager;
    }

    public async Task<RubbergodServiceInfo> ProcessAsync()
    {
        var info = new RubbergodServiceInfo
        {
            Timeout = RubbergodServiceClient.Timeout,
            Url = RubbergodServiceClient.Url
        };

        try
        {
            info.Info = await RubbergodServiceClient.GetDiagAsync();
        }
        catch (Exception ex)
        {
            await LoggingManager.ErrorAsync("API", "An error occured while loading diagnostics info from Rubergod service", ex);
            info.ApiErrorMessage = ex.Message;
        }

        return info;
    }
}
