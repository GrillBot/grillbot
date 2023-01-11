using System.Net.Http;
using System.Net.Http.Json;
using GrillBot.App.Services.Graphics.Models.Chart;
using GrillBot.App.Services.Graphics.Models.Diagnostics;
using GrillBot.Common.Managers.Counters;

namespace GrillBot.App.Services.Graphics;

public class GraphicsClient : IGraphicsClient
{
    private HttpClient HttpClient { get; }
    private CounterManager CounterManager { get; }

    public GraphicsClient(IHttpClientFactory httpClientFactory, CounterManager counterManager)
    {
        HttpClient = httpClientFactory.CreateClient("Graphics");
        CounterManager = counterManager;
    }

    public async Task<byte[]> CreateChartAsync(ChartRequestData request)
    {
        using (CounterManager.Create("Service.Graphics"))
        {
            using var response = await HttpClient.PostAsJsonAsync("chart", request);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsByteArrayAsync();
            throw await HandleInvalidRequestAsync(response);
        }
    }

    public async Task<Metrics> GetMetricsAsync()
    {
        using (CounterManager.Create("Service.Graphics"))
        {
            using var response = await HttpClient.GetAsync("metrics");
            if (!response.IsSuccessStatusCode)
                throw await HandleInvalidRequestAsync(response);

            var data = await response.Content.ReadAsStringAsync();
            var jsonData = JObject.Parse(data);
            return new Metrics
            {
                Uptime = TimeSpan.FromMilliseconds(jsonData["uptime"]!.Value<double>()),
                UsedMemory = jsonData["mem"]!["rss"]!.Value<long>()
            };
        }
    }

    public async Task<string> GetVersionAsync()
    {
        using (CounterManager.Create("Service.Graphics"))
        {
            using var response = await HttpClient.GetAsync("info");
            if (!response.IsSuccessStatusCode)
                throw await HandleInvalidRequestAsync(response);

            var data = await response.Content.ReadAsStringAsync();
            return JObject.Parse(data)["build"]!["version"]!.Value<string>()!;
        }
    }

    private static async Task<Exception> HandleInvalidRequestAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return new HttpRequestException(content, null, response.StatusCode);
    }
}
