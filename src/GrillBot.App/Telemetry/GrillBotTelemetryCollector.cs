using GrillBot.Common.Managers.Events;
using GrillBot.Core.Metrics.Collectors;
using GrillBot.Core.Metrics.Components;

namespace GrillBot.App.Telemetry;

public class GrillBotTelemetryCollector : ITelemetryCollector
{
    public CommandsTelemetryCounter Commands { get; } = new();
    public TelemetryGaugeContainer WebsocketEvents { get; } = new("websocket_events", "Websocket counters since bot starts.");

    public GrillBotTelemetryCollector(EventLogManager _eventLogManager)
    {
        _eventLogManager.OnEventReceived = (eventName, count) => WebsocketEvents.Set(eventName, count, new() { { "event", eventName } });
    }

    public IEnumerable<TelemetryCollectorComponent> GetComponents()
    {
        yield return Commands;
        yield return WebsocketEvents;
    }
}
