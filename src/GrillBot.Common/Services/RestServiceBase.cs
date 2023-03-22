using System.Net;
using System.Net.Http.Json;
using GrillBot.Core.Managers.Performance;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.Common.Services;

public abstract class RestServiceBase
{
    protected HttpClient HttpClient { get; }
    private ICounterManager CounterManager { get; }
    public abstract string ServiceName { get; }

    public string Url => HttpClient.BaseAddress!.ToString();
    public int Timeout => Convert.ToInt32(HttpClient.Timeout.TotalMilliseconds);

    protected static readonly Task<object?> EmptyResult = Task.FromResult((object?)null);

    protected RestServiceBase(ICounterManager counterManager, Func<HttpClient> clientFactory)
    {
        CounterManager = counterManager;
        HttpClient = clientFactory();
    }

    protected async Task<TResult> ProcessRequestAsync<TResult>(Func<Task<HttpResponseMessage>> executeRequest, Func<HttpResponseMessage, Task<TResult>> fetchResult,
        Func<HttpResponseMessage, Task>? checkResponse = null)
    {
        using (CounterManager.Create($"Service.{ServiceName}"))
        {
            using var response = await executeRequest();

            if (checkResponse != null)
                await checkResponse(response);
            else
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

    protected static async Task<ValidationProblemDetails?> DesrializeValidationErrorsAsync(HttpResponseMessage response)
    {
        if (response.StatusCode != HttpStatusCode.BadRequest) return null;
        return await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
    }
}
