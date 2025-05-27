using GrillBot.App.Services.Discord;
using GrillBot.Core.Metrics.CustomTelemetry;
using System.Diagnostics.Metrics;

namespace GrillBot.App.Telemetry;

public class CommandsTelemetryBuilder : ICustomTelemetryBuilder
{
    public void BuildCustomTelemetry(Meter meter)
    {
        meter.CreateObservableCounter("grillbot_commands", CommandsPerformanceCounter.GetCount, description: "Count of executed commands.");
    }
}
