using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Counters;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.System;
using Microsoft.AspNetCore.Hosting;

namespace GrillBot.App.Actions.Api.V1.System;

public class GetDiagnostics : ApiAction
{
    private InitManager InitManager { get; }
    private CounterManager CounterManager { get; }
    private IWebHostEnvironment WebHostEnvironment { get; }
    private IDiscordClient DiscordClient { get; }

    public GetDiagnostics(ApiRequestContext apiContext, InitManager initManager, CounterManager counterManager, IWebHostEnvironment environment, IDiscordClient discordClient) : base(apiContext)
    {
        InitManager = initManager;
        CounterManager = counterManager;
        WebHostEnvironment = environment;
        DiscordClient = discordClient;
    }

    public DiagnosticsInfo Process()
    {
        var isActive = InitManager.Get();
        var activeOperations = CounterManager.GetActiveCounters();

        return new DiagnosticsInfo(WebHostEnvironment.EnvironmentName, DiscordClient.ConnectionState, isActive, activeOperations);
    }
}
