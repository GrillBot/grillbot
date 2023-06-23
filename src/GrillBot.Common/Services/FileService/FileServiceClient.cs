using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GrillBot.Core.Managers.Performance;
using GrillBot.Core.Services.Diagnostics.Models;

namespace GrillBot.Common.Services.FileService;

public class FileServiceClient : RestServiceBase, IFileServiceClient
{
    public override string ServiceName => "FileService";

    public FileServiceClient(ICounterManager counterManager, IHttpClientFactory clientFactory) : base(counterManager, () => clientFactory.CreateClient("FileService"))
    {
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
            async response => response.StatusCode == HttpStatusCode.NotFound ? null : await response.Content.ReadAsByteArrayAsync(),
            async response =>
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return;
                await EnsureSuccessResponseAsync(response);
            }
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
