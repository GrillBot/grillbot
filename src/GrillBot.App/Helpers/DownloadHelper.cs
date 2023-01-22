using System.Net.Http;
using GrillBot.Common.Managers.Counters;

namespace GrillBot.App.Helpers;

public class DownloadHelper
{
    private CounterManager CounterManager { get; }
    private IHttpClientFactory HttpClientFactory { get; }

    public DownloadHelper(CounterManager counterManager, IHttpClientFactory httpClientFactory)
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

    public async Task<byte[]?> DownloadFileAsync(string url)
    {
        var httpClient = HttpClientFactory.CreateClient();

        using (CounterManager.Create("FileDownload"))
        {
            try
            {
                return await httpClient.GetByteArrayAsync(url);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }
    }
}
