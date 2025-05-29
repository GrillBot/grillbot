using GrillBot.Core.Metrics.Collectors;
using GrillBot.Core.Metrics.Components;

namespace GrillBot.App.Telemetry;

public class GrillBotTelemetryCollector : ITelemetryCollector
{
    public CommandsTelemetryCounter Commands { get; } = new();

    public IEnumerable<TelemetryCollectorComponent> GetComponents()
    {
        yield return Commands;
    }
}
