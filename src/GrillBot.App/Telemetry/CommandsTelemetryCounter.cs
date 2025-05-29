using GrillBot.App.Services.Discord;
using GrillBot.Core.Metrics.Components;
using System.Diagnostics.Metrics;

namespace GrillBot.App.Telemetry;

public class CommandsTelemetryCounter : TelemetryCounter
{
    public CommandsTelemetryCounter() : base("grillbot_commands", null, "Count of executed commands.")
    {
    }

    public override Instrument CreateInstrument(Meter meter)
        => meter.CreateObservableCounter(Name, CommandsPerformanceCounter.GetCount, description: Description);
}
