using GrillBot.Common.Services.Graphics.Models.Diagnostics;

namespace GrillBot.Data.Models.API.Services;

public class GraphicsServiceInfo : ServiceInfoBase
{
    public Metrics? Metrics { get; set; }
    public string? Version { get; set; }
    public Stats? Statistics { get; set; }
}
