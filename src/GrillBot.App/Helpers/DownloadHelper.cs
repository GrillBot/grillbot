using System.Net.Http;
using GrillBot.Core.Managers.Performance;

namespace GrillBot.App.Helpers;

public class DownloadHelper
{
    private ICounterManager CounterManager { get; }
    private IHttpClientFactory HttpClientFactory { get; }

    public DownloadHelper(ICounterManager counterManager, IHttpClientFactory httpClientFactory)
    {
        CounterManager = counterManager;
        HttpClientFactory = httpClientFactory;
    }

    public async Task<byte[]?> DownloadAsync(IAttachment attachment)
    {
        var httpClient = HttpClientFactory.CreateClient();

        using (CounterManager.Create("FileDownload"))
        {
            try
            {
                return await httpClient.GetByteArrayAsync(attachment.Url);
            }
            catch (HttpRequestException)
            {
                try
                {
                    return await httpClient.GetByteArrayAsync(attachment.ProxyUrl);
                }
                catch (HttpRequestException)
                {
                    return null;
                }
            }
        }
    }

    public async Task<byte[]?> DownloadFileAsync(string url, CancellationToken cancellationToken = default)
    {
        var httpClient = HttpClientFactory.CreateClient();

        using (CounterManager.Create("FileDownload"))
        {
            try
            {
                return await httpClient.GetByteArrayAsync(url, cancellationToken);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }
    }
}
