using System.Net.Http.Json;
using GrillBot.Common.Managers.Counters;
using GrillBot.Common.Services.Graphics.Models.Chart;
using GrillBot.Common.Services.Graphics.Models.Diagnostics;
using Newtonsoft.Json.Linq;

namespace GrillBot.Common.Services.Graphics;

public class GraphicsClient : IGraphicsClient
{
    public string Url => HttpClient.BaseAddress!.ToString();
    public int Timeout => Convert.ToInt32(HttpClient.Timeout.TotalMilliseconds);

    private HttpClient HttpClient { get; }
    private CounterManager CounterManager { get; }

    public GraphicsClient(IHttpClientFactory httpClientFactory, CounterManager counterManager)
    {
        HttpClient = httpClientFactory.CreateClient("Graphics");
        CounterManager = counterManager;
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            await GetVersionAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
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
                Uptime = (long)Math.Ceiling(jsonData["uptime"]!.Value<double>() * 1000),
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

    public async Task<Stats> GetStatisticsAsync()
    {
        using (CounterManager.Create("Service.Graphics"))
        {
            using var response = await HttpClient.GetAsync("stats");
            if (!response.IsSuccessStatusCode)
                throw await HandleInvalidRequestAsync(response);

            return (await response.Content.ReadFromJsonAsync<Stats>())!;
        }
    }

    private static async Task<Exception> HandleInvalidRequestAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        var errorMessage = $"API returned status code {response.StatusCode}\n{content}";

        return new HttpRequestException(errorMessage, null, response.StatusCode);
    }
}
