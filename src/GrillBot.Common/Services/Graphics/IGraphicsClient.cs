using GrillBot.Common.Services.Graphics.Models.Chart;
using GrillBot.Common.Services.Graphics.Models.Diagnostics;
using GrillBot.Common.Services.Graphics.Models.Images;

namespace GrillBot.Common.Services.Graphics;

public interface IGraphicsClient
{
    string Url { get; }
    int Timeout { get; }
    
    Task<bool> IsAvailableAsync();
    Task<byte[]> CreateChartAsync(ChartRequestData request);
    Task<Metrics> GetMetricsAsync();
    Task<string> GetVersionAsync();
    Task<Stats> GetStatisticsAsync();
    Task<byte[]> CreateWithoutAccidentImage(WithoutAccidentRequestData request);
}
