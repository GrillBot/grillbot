using GrillBot.Common.Managers;
using GrillBot.Common.Models;
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

    public Data.Models.API.System.DashboardInfo Process()
    {
        var process = global::System.Diagnostics.Process.GetCurrentProcess();

        return new Data.Models.API.System.DashboardInfo
        {
            Uptime = Convert.ToInt64((DateTime.Now - process.StartTime).TotalMilliseconds),
            ConnectionState = DiscordClient.ConnectionState,
            UsedMemory = process.WorkingSet64,
            IsActive = InitManager.Get(),
            CurrentDateTime = DateTime.Now,
            CpuTime = Convert.ToInt64(process.TotalProcessorTime.TotalMilliseconds),
            StartAt = process.StartTime,
            IsDevelopment = WebHost.IsDevelopment()
        };
    }
}
