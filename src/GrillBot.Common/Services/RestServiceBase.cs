using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using GrillBot.Common.Services.Common;
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

    protected RestServiceBase(ICounterManager counterManager, IHttpClientFactory httpClientFactory)
    {
        CounterManager = counterManager;
        HttpClient = httpClientFactory.CreateClient(ServiceName);
    }

    protected async Task<TResult> ProcessRequestAsync<TResult>(
        Func<CancellationToken, Task<HttpResponseMessage>> executeRequest,
        Func<HttpResponseMessage, CancellationToken, Task<TResult?>> fetchResult,
        Func<HttpResponseMessage, CancellationToken, Task>? checkResponse = null,
        TimeSpan? timeout = null
    )
    {
        timeout ??= HttpClient.Timeout;

        using (CounterManager.Create($"Service.{ServiceName}"))
        {
            using var cancellationTokenSource = new CancellationTokenSource(timeout.Value);
            using var response = await ExecuteRequestAsync(executeRequest, cancellationTokenSource.Token);

            if (checkResponse != null)
                await checkResponse(response, cancellationTokenSource.Token);
            else
                await EnsureSuccessResponseAsync(response, cancellationTokenSource.Token);
            return (await fetchResult(response, cancellationTokenSource.Token))!;
        }
    }

    private static async Task<HttpResponseMessage> ExecuteRequestAsync(Func<CancellationToken, Task<HttpResponseMessage>> executeRequest, CancellationToken cancellationToken, bool isRepeat = false)
    {
        try
        {
            return await executeRequest(cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.InnerException is SocketException socketException &&
                                              (socketException.NativeErrorCode == 111 || socketException.SocketErrorCode == SocketError.ConnectionRefused) && !isRepeat)
        {
            await Task.Delay(1000); // Wait 1 second to repeat request execution.
            return await ExecuteRequestAsync(executeRequest, cancellationToken, true);
        }
    }

    protected static async Task EnsureSuccessResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode) return;

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var errorMessage = $"API returned status code {response.StatusCode}\n{content}";
        throw new HttpRequestException(errorMessage, null, response.StatusCode) { Data = { { "ResponseContent", content } } };
    }

    protected static async Task<ValidationProblemDetails?> DeserializeValidationErrorsAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.StatusCode != HttpStatusCode.BadRequest) return null;
        return await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken: cancellationToken);
    }

    protected static async Task<TResult?> ReadJsonAsync<TResult>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.NoContent)
            return default;

        return await response.Content.ReadFromJsonAsync<TResult>(cancellationToken: cancellationToken);
    }

    protected static async Task<RestResponse<TResult>?> ReadRestResponseAsync<TResult>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var validationError = await DeserializeValidationErrorsAsync(response, cancellationToken);
        return validationError is not null
            ? new RestResponse<TResult>(validationError)
            : new RestResponse<TResult>(await ReadJsonAsync<TResult>(response, cancellationToken));
    }

    protected static Task<object?> EmptyResponseAsync(HttpResponseMessage _, CancellationToken __)
        => Task.FromResult((object?)null);

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            await ProcessRequestAsync(
                async cancellationToken =>
                {
                    using var message = new HttpRequestMessage(HttpMethod.Head, "health");
                    return await HttpClient.SendAsync(message, cancellationToken);
                },
                EmptyResponseAsync
            );

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
