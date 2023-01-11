using GrillBot.App.Services.Graphics.Models.Chart;
using GrillBot.App.Services.Graphics.Models.Diagnostics;

namespace GrillBot.App.Services.Graphics;

public interface IGraphicsClient
{
    Task<byte[]> CreateChartAsync(ChartRequestData request);
    Task<Metrics> GetMetricsAsync();
    Task<string> GetVersionAsync();
}
