using System.Net;
using System.Net.Http.Headers;
using GrillBot.Core.Managers.Performance;
using GrillBot.Core.Services.Diagnostics.Models;

namespace GrillBot.Common.Services.FileService;

public class FileServiceClient : RestServiceBase, IFileServiceClient
{
    public override string ServiceName => "FileService";

    public FileServiceClient(ICounterManager counterManager, IHttpClientFactory clientFactory) : base(counterManager, clientFactory)
    {
    }

    public async Task<DiagnosticInfo> GetDiagAsync()
        => await ProcessRequestAsync(cancellationToken => HttpClient.GetAsync("api/diag", cancellationToken), ReadJsonAsync<DiagnosticInfo>);

    public async Task UploadFileAsync(string filename, byte[] content, string contentType)
    {
        await ProcessRequestAsync(
            cancellationToken =>
            {
                var fileContent = new ByteArrayContent(content);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);

                var formData = new MultipartFormDataContent
                {
                    { fileContent, "file", filename }
                };
                return HttpClient.PostAsync("api/data", formData, cancellationToken);
            },
            EmptyResponseAsync,
            timeout: System.Threading.Timeout.InfiniteTimeSpan
        );
    }

    public async Task<byte[]?> DownloadFileAsync(string filename)
    {
        return await ProcessRequestAsync(
            cancellationToken => HttpClient.GetAsync($"api/data?filename={filename}", cancellationToken),
            async (response, cancellationToken) => response.StatusCode == HttpStatusCode.NotFound ? null : await response.Content.ReadAsByteArrayAsync(cancellationToken: cancellationToken),
            (response, cancellationToken) => response.StatusCode == HttpStatusCode.NotFound ? Task.CompletedTask : EnsureSuccessResponseAsync(response, cancellationToken)
        );
    }

    public async Task DeleteFileAsync(string filename)
    {
        await ProcessRequestAsync(
            cancellationToken => HttpClient.DeleteAsync($"api/data?filename={filename}", cancellationToken),
            EmptyResponseAsync
        );
    }

    public async Task<string?> GenerateLinkAsync(string filename)
    {
        return await ProcessRequestAsync(
            cancellationToken => HttpClient.GetAsync($"api/data/link?filename={filename}", cancellationToken),
            (response, cancellationToken) => response.Content.ReadAsStringAsync(cancellationToken: cancellationToken)!
        );
    }
}
