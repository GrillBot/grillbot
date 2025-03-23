using GrillBot.Common.Extensions;
using GrillBot.Common.Managers;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Models.API.System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace GrillBot.App.Actions.Api.V3.Dashboard;

public class GetBotCommonInfoAction(
    ApiRequestContext apiContext,
    IDiscordClient _discordClient,
    InitManager _initManager,
    IWebHostEnvironment _webHost,
    IConfiguration _configuration
) : ApiAction(apiContext)
{
    public override Task<ApiResult> ProcessAsync()
    {
        var currentProcess = Process.GetCurrentProcess();
        var now = DateTime.Now;

        var result = new DashboardInfo
        {
            ConnectionState = _discordClient.ConnectionState,
            CpuTime = currentProcess.TotalProcessorTime.ToTotalMiliseconds(),
            CurrentDateTime = now,
            IsActive = _initManager.Get(),
            IsDevelopment = _webHost.IsDevelopment(),
            StartAt = currentProcess.StartTime.ToUniversalTime(),
            Uptime = (now - currentProcess.StartTime).ToTotalMiliseconds(),
            UsedMemory = currentProcess.WorkingSet64,
            Services = GetServicesList()
        };

        return Task.FromResult(ApiResult.Ok(result));
    }

    private List<string> GetServicesList()
    {
        return [.. _configuration
            .GetRequiredSection("Services")
            .AsEnumerable()
            .Where(o => o.Key.Contains(':') && o.Value is null)
            .Select(o => o.Key.Replace("Services:", "").Trim())
            .Where(o => !_configuration.GetValue<bool>($"Services:{o}:IsThirdParty"))
            .Order()
        ];
    }
}
