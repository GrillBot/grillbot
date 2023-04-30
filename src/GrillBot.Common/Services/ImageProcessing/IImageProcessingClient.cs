using GrillBot.Common.Services.Common;
using GrillBot.Common.Services.ImageProcessing.Models;
using GrillBot.Core.Services.Diagnostics.Models;

namespace GrillBot.Common.Services.ImageProcessing;

public interface IImageProcessingClient : IClient
{
    Task<DiagnosticInfo> GetDiagAsync();
    Task<byte[]> CreatePeepoloveImageAsync(PeepoRequest request);
    Task<byte[]> CreatePeepoangryImageAsync(PeepoRequest request);
    Task<byte[]> CreatePointsImageAsync(PointsRequest request);
    Task<byte[]> CreateWithoutAccidentImageAsync(WithoutAccidentImageRequest request);
    Task<byte[]> CreateChartImageAsync(ChartRequest request);
}
