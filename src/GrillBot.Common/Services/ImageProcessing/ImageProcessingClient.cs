using System.Net.Http.Json;
using GrillBot.Common.Services.ImageProcessing.Models;
using GrillBot.Core.Managers.Performance;
using GrillBot.Core.Services.Diagnostics.Models;

namespace GrillBot.Common.Services.ImageProcessing;

public class ImageProcessingClient : RestServiceBase, IImageProcessingClient
{
    public override string ServiceName => "ImageProcessing";

    public ImageProcessingClient(ICounterManager counterManager, IHttpClientFactory httpClientFactory) : base(counterManager, () => httpClientFactory.CreateClient("ImageProcessing"))
    {
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            await ProcessRequestAsync(
                () => HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, "health")),
                _ => EmptyResult
            );

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<DiagnosticInfo> GetDiagAsync()
    {
        return (await ProcessRequestAsync(
            () => HttpClient.GetAsync("api/diag"),
            response => response.Content.ReadFromJsonAsync<DiagnosticInfo>()
        ))!;
    }

    public async Task<byte[]> CreatePeepoloveImageAsync(PeepoRequest request)
    {
        return await ProcessRequestAsync(
            () => HttpClient.PostAsJsonAsync("api/image/peepolove", request),
            response => response.Content.ReadAsByteArrayAsync()
        );
    }

    public async Task<byte[]> CreatePeepoangryImageAsync(PeepoRequest request)
    {
        return await ProcessRequestAsync(
            () => HttpClient.PostAsJsonAsync("api/image/peepoangry", request),
            response => response.Content.ReadAsByteArrayAsync()
        );
    }

    public async Task<byte[]> CreatePointsImageAsync(PointsRequest request)
    {
        return await ProcessRequestAsync(
            () => HttpClient.PostAsJsonAsync("api/image/points", request),
            response => response.Content.ReadAsByteArrayAsync()
        );
    }

    public async Task<byte[]> CreateWithoutAccidentImageAsync(WithoutAccidentImageRequest request)
    {
        return await ProcessRequestAsync(
            () => HttpClient.PostAsJsonAsync("api/image/without-accident", request),
            response => response.Content.ReadAsByteArrayAsync()
        );
    }

    public async Task<byte[]> CreateChartImageAsync(ChartRequest request)
    {
        return await ProcessRequestAsync(
            () => HttpClient.PostAsJsonAsync("api/image/chart", request),
            response => response.Content.ReadAsByteArrayAsync()
        );
    }
}
