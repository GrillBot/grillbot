using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GrillBot.Common.Managers.Counters;
using GrillBot.Common.Services.Math.Models;

namespace GrillBot.Common.Services.Math;

public class MathClient : RestServiceBase, IMathClient
{
    public override string ServiceName => "MathJS";

    public MathClient(CounterManager counterManager, IHttpClientFactory httpClientFactory) : base(counterManager, () => httpClientFactory.CreateClient("Math"))
    {
    }

    public async Task<MathJsResult> SolveExpressionAsync(MathJsRequest request)
    {
        try
        {
            var result = await ProcessRequestAsync(
                () => HttpClient.PostAsJsonAsync("", request),
                response => response.Content.ReadFromJsonAsync<MathJsResult>()
            );

            return result!;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            var response = ex.Data["ResponseContent"] as string;
            if (string.IsNullOrEmpty(response) || !response.StartsWith("{"))
                throw;

            return JsonSerializer.Deserialize<MathJsResult>(response)!;
        }
        catch (TaskCanceledException)
        {
            return new MathJsResult { IsTimeout = true };
        }
    }
}
