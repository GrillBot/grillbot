using System.Net.Http.Json;
using GrillBot.Common.Services.Graphics.Models.Chart;
using GrillBot.Common.Services.Graphics.Models.Diagnostics;
using GrillBot.Common.Services.Graphics.Models.Images;
using GrillBot.Core.Managers.Performance;
using Newtonsoft.Json.Linq;

namespace GrillBot.Common.Services.Graphics;

public class GraphicsClient : RestServiceBase, IGraphicsClient
{
    public override string ServiceName => "Graphics";

    public GraphicsClient(IHttpClientFactory httpClientFactory, ICounterManager counterManager) : base(counterManager, httpClientFactory)
    {
    }

    public async Task<byte[]> CreateChartAsync(ChartRequestData request)
    {
        return await ProcessRequestAsync(
            cancellationToken => HttpClient.PostAsJsonAsync("chart", request, cancellationToken),
            (response, cancellationToken) => response.Content.ReadAsByteArrayAsync(cancellationToken: cancellationToken)!
        );
    }

    public async Task<Metrics> GetMetricsAsync()
    {
        return await ProcessRequestAsync(
            cancellationToken => HttpClient.GetAsync("metrics", cancellationToken),
            async (response, cancellationToken) =>
            {
                var json = JObject.Parse(await response.Content.ReadAsStringAsync(cancellationToken: cancellationToken));
                return new Metrics
                {
                    Uptime = (long)System.Math.Ceiling(json["uptime"]!.Value<double>() * 1000),
                    UsedMemory = json["mem"]!["rss"]!.Value<long>()
                };
            }
        );
    }

    public async Task<string> GetVersionAsync()
    {
        return await ProcessRequestAsync(
            cancellationToken => HttpClient.GetAsync("info", cancellationToken),
            async (response, cancellationToken) => JObject.Parse(await response.Content.ReadAsStringAsync(cancellationToken: cancellationToken))["build"]!["version"]!.Value<string>()!
        );
    }

    public async Task<Stats> GetStatisticsAsync()
        => await ProcessRequestAsync(cancellationToken => HttpClient.GetAsync("stats", cancellationToken), ReadJsonAsync<Stats>);

    public async Task<byte[]> CreateWithoutAccidentImage(WithoutAccidentRequestData request)
    {
        return await ProcessRequestAsync(
            cancellationToken => HttpClient.PostAsJsonAsync("image/without-accident", request, cancellationToken),
            (response, cancellationToken) => response.Content.ReadAsByteArrayAsync(cancellationToken: cancellationToken)!
        );
    }

    public async Task<byte[]> CreatePointsImageAsync(PointsImageRequest imageRequest)
    {
        return await ProcessRequestAsync(
            cancellationToken => HttpClient.PostAsJsonAsync("image/points", imageRequest, cancellationToken),
            (response, cancellationToken) => response.Content.ReadAsByteArrayAsync(cancellationToken: cancellationToken)!
        );
    }

    public async Task<List<byte[]>> CreatePeepoAngryAsync(List<byte[]> avatarFrames)
        => await ProcessRequestAsync(cancellationToken => HttpClient.PostAsJsonAsync("image/peepo/angry", avatarFrames, cancellationToken), ReadJsonAsync<List<byte[]>>);

    public async Task<List<byte[]>> CreatePeepoLoveAsync(List<byte[]> avatarFrames)
        => await ProcessRequestAsync(cancellationToken => HttpClient.PostAsJsonAsync("image/peepo/love", avatarFrames, cancellationToken), ReadJsonAsync<List<byte[]>>);
}
