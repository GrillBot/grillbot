using GrillBot.Common.Managers;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace GrillBot.App.Actions.Api.V1.Dashboard;

public class GetCommonInfo : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private InitManager InitManager { get; }
    private IWebHostEnvironment WebHost { get; }

    public GetCommonInfo(ApiRequestContext apiContext, IDiscordClient discordClient, InitManager initManager, IWebHostEnvironment webHost) : base(apiContext)
    {
        DiscordClient = discordClient;
        InitManager = initManager;
        WebHost = webHost;
    }

    public override Task<ApiResult> ProcessAsync()
    {
        var process = global::System.Diagnostics.Process.GetCurrentProcess();
        var now = DateTime.Now;

        var result = new Data.Models.API.System.DashboardInfo
        {
            Uptime = Convert.ToInt64((now - process.StartTime).TotalMilliseconds),
            ConnectionState = DiscordClient.ConnectionState,
            UsedMemory = process.WorkingSet64,
            IsActive = InitManager.Get(),
            CurrentDateTime = DateTime.Now,
            CpuTime = Convert.ToInt64(process.TotalProcessorTime.TotalMilliseconds),
            StartAt = process.StartTime,
            IsDevelopment = WebHost.IsDevelopment()
        };

        return Task.FromResult(ApiResult.Ok(result));
    }
}
