﻿using System.Net.Http.Json;
using GrillBot.Common.Services.Graphics.Models.Chart;
using GrillBot.Common.Services.Graphics.Models.Diagnostics;
using GrillBot.Common.Services.Graphics.Models.Images;
using GrillBot.Core.Managers.Performance;
using Newtonsoft.Json.Linq;

namespace GrillBot.Common.Services.Graphics;

public class GraphicsClient : RestServiceBase, IGraphicsClient
{
    public override string ServiceName => "Graphics";

    public GraphicsClient(IHttpClientFactory httpClientFactory, ICounterManager counterManager) : base(counterManager, () => httpClientFactory.CreateClient("Graphics"))
    {
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            await ProcessRequestAsync(
                () => HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, "health")),
                _ => Task.FromResult((object?)null)
            );

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<byte[]> CreateChartAsync(ChartRequestData request)
    {
        return await ProcessRequestAsync(
            () => HttpClient.PostAsJsonAsync("chart", request),
            response => response.Content.ReadAsByteArrayAsync()
        );
    }

    public async Task<Metrics> GetMetricsAsync()
    {
        return await ProcessRequestAsync(
            () => HttpClient.GetAsync("metrics"),
            async response =>
            {
                var json = JObject.Parse(await response.Content.ReadAsStringAsync());
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
            () => HttpClient.GetAsync("info"),
            async response => JObject.Parse(await response.Content.ReadAsStringAsync())["build"]!["version"]!.Value<string>()!
        );
    }

    public async Task<Stats> GetStatisticsAsync()
    {
        return (await ProcessRequestAsync(
            () => HttpClient.GetAsync("stats"),
            response => response.Content.ReadFromJsonAsync<Stats>()
        ))!;
    }

    public async Task<byte[]> CreateWithoutAccidentImage(WithoutAccidentRequestData request)
    {
        return await ProcessRequestAsync(
            () => HttpClient.PostAsJsonAsync("image/without-accident", request),
            response => response.Content.ReadAsByteArrayAsync()
        );
    }

    public async Task<byte[]> CreatePointsImageAsync(PointsImageRequest request)
    {
        return await ProcessRequestAsync(
            () => HttpClient.PostAsJsonAsync("image/points", request),
            response => response.Content.ReadAsByteArrayAsync()
        );
    }

    public async Task<List<byte[]>> CreatePeepoAngryAsync(List<byte[]> avatarFrames)
    {
        var result = await ProcessRequestAsync(
            () => HttpClient.PostAsJsonAsync("image/peepo/angry", avatarFrames),
            response => response.Content.ReadFromJsonAsync<List<byte[]>>()
        );

        return result!;
    }

    public async Task<List<byte[]>> CreatePeepoLoveAsync(List<byte[]> avatarFrames)
    {
        var result = await ProcessRequestAsync(
            () => HttpClient.PostAsJsonAsync("image/peepo/love", avatarFrames),
            response => response.Content.ReadFromJsonAsync<List<byte[]>>()
        );

        return result!;
    }
}
