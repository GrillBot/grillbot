using System.Net.Http.Json;
using GrillBot.Common.Services.ImageProcessing.Models;
using GrillBot.Core.Managers.Performance;
using GrillBot.Core.Services.Diagnostics.Models;

namespace GrillBot.Common.Services.ImageProcessing;

public class ImageProcessingClient : RestServiceBase, IImageProcessingClient
{
    public override string ServiceName => "ImageProcessing";

    public ImageProcessingClient(ICounterManager counterManager, IHttpClientFactory httpClientFactory) : base(counterManager, httpClientFactory)
    {
    }

    public async Task<DiagnosticInfo> GetDiagAsync()
        => await ProcessRequestAsync(cancellationToken => HttpClient.GetAsync("api/diag", cancellationToken), ReadJsonAsync<DiagnosticInfo>);

    public async Task<byte[]> CreatePeepoloveImageAsync(PeepoRequest request)
    {
        return await ProcessRequestAsync(
            cancellationToken => HttpClient.PostAsJsonAsync("api/image/peepolove", request, cancellationToken),
            (response, cancellationToken) => response.Content.ReadAsByteArrayAsync(cancellationToken: cancellationToken)!
        );
    }

    public async Task<byte[]> CreatePeepoangryImageAsync(PeepoRequest request)
    {
        return await ProcessRequestAsync(
            cancellationToken => HttpClient.PostAsJsonAsync("api/image/peepoangry", request, cancellationToken),
            (response, cancellationToken) => response.Content.ReadAsByteArrayAsync(cancellationToken: cancellationToken)!
        );
    }

    public async Task<byte[]> CreatePointsImageAsync(PointsRequest request)
    {
        return await ProcessRequestAsync(
            cancellationToken => HttpClient.PostAsJsonAsync("api/image/points", request, cancellationToken),
            (response, cancellationToken) => response.Content.ReadAsByteArrayAsync(cancellationToken: cancellationToken)!
        );
    }

    public async Task<byte[]> CreateWithoutAccidentImageAsync(WithoutAccidentImageRequest request)
    {
        return await ProcessRequestAsync(
            cancellationToken => HttpClient.PostAsJsonAsync("api/image/without-accident", request, cancellationToken),
            (response, cancellationToken) => response.Content.ReadAsByteArrayAsync(cancellationToken: cancellationToken)!
        );
    }

    public async Task<byte[]> CreateChartImageAsync(ChartRequest request)
    {
        return await ProcessRequestAsync(
            cancellationToken => HttpClient.PostAsJsonAsync("api/image/chart", request, cancellationToken),
            (response, cancellationToken) => response.Content.ReadAsByteArrayAsync(cancellationToken: cancellationToken)!
        );
    }
}
