using GrillBot.Common.Managers.Counters;

namespace GrillBot.Common.Services;

public abstract class RestServiceBase
{
    protected HttpClient HttpClient { get; }
    private CounterManager CounterManager { get; }
    protected abstract string ServiceName { get; }

    public string Url => HttpClient.BaseAddress!.ToString();
    public int Timeout => Convert.ToInt32(HttpClient.Timeout.TotalMilliseconds);

    protected static readonly Task<object?> EmptyResult = Task.FromResult((object?)null);

    protected RestServiceBase(CounterManager counterManager, Func<HttpClient> clientFactory)
    {
        CounterManager = counterManager;
        HttpClient = clientFactory();
    }

    protected async Task<TResult> ProcessRequestAsync<TResult>(Func<Task<HttpResponseMessage>> executeRequest, Func<HttpResponseMessage, Task<TResult>> fetchResult)
    {
        using (CounterManager.Create($"Service.{ServiceName}"))
        {
            using var response = await executeRequest();

            await EnsureSuccessResponseAsync(response);
            return await fetchResult(response);
        }
    }

    protected static async Task EnsureSuccessResponseAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        var content = await response.Content.ReadAsStringAsync();
        var errorMessage = $"API returned status code {response.StatusCode}\n{content}";
        throw new HttpRequestException(errorMessage, null, response.StatusCode) { Data = { { "ResponseContent", content } } };
    }
}
