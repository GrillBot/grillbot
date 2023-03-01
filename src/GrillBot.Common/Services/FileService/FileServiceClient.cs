using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GrillBot.Common.Managers.Counters;
using GrillBot.Common.Services.Common.Models.Diagnostics;

namespace GrillBot.Common.Services.FileService;

public class FileServiceClient : RestServiceBase, IFileServiceClient
{
    protected override string ServiceName => "FileService";

    public FileServiceClient(CounterManager counterManager, IHttpClientFactory clientFactory) : base(counterManager, () => clientFactory.CreateClient("FileService"))
    {
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            await ProcessRequestAsync(
                () => HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, "health")),
                _ => EmptyResult
            );

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<DiagnosticInfo> GetDiagAsync()
    {
        return (await ProcessRequestAsync(
            () => HttpClient.GetAsync("api/diag"),
            response => response.Content.ReadFromJsonAsync<DiagnosticInfo>()
        ))!;
    }

    public async Task UploadFileAsync(string filename, byte[] content, string contentType)
    {
        await ProcessRequestAsync(
            () =>
            {
                var fileContent = new ByteArrayContent(content);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);

                var formData = new MultipartFormDataContent();
                formData.Add(fileContent, "file", filename);
                return HttpClient.PostAsync("api/data", formData);
            },
            _ => EmptyResult
        );
    }

    public async Task<byte[]?> DownloadFileAsync(string filename)
    {
        return await ProcessRequestAsync(
            () => HttpClient.GetAsync($"api/data?filename={filename}"),
            async response => response.StatusCode == HttpStatusCode.NotFound ? null : await response.Content.ReadAsByteArrayAsync()
        );
    }

    public async Task DeleteFileAsync(string filename)
    {
        await ProcessRequestAsync(
            () => HttpClient.DeleteAsync($"api/data?filename={filename}"),
            _ => EmptyResult
        );
    }

    public async Task<string?> GenerateLinkAsync(string filename)
    {
        return await ProcessRequestAsync(
            () => HttpClient.GetAsync($"api/data/link?filename={filename}"),
            response => response.Content.ReadAsStringAsync()
        );
    }
}
