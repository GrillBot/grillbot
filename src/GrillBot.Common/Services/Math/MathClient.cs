using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GrillBot.Common.Services.Math.Models;
using GrillBot.Core.Managers.Performance;
using GrillBot.Core.Services;

namespace GrillBot.Common.Services.Math;

public class MathClient : RestServiceBase, IMathClient
{
    public override string ServiceName => "MathJS";

    public MathClient(ICounterManager counterManager, IHttpClientFactory httpClientFactory) : base(counterManager, httpClientFactory)
    {
    }

    public async Task<MathJsResult> SolveExpressionAsync(MathJsRequest request)
    {
        try
        {
            return await ProcessRequestAsync(cancellationToken => HttpClient.PostAsJsonAsync("", request, cancellationToken), ReadJsonAsync<MathJsResult>);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            var response = ex.Data["ResponseContent"] as string;
            if (string.IsNullOrEmpty(response) || !response.StartsWith('{'))
                throw;

            return JsonSerializer.Deserialize<MathJsResult>(response)!;
        }
        catch (TaskCanceledException)
        {
            return new MathJsResult { IsTimeout = true };
        }
    }
}
