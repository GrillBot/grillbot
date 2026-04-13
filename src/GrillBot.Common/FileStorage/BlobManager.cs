using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using GrillBot.Core.Managers.Performance;

namespace GrillBot.Common.FileStorage;

public class BlobManager(
    ICounterManager _counterManager,
    BlobContainerClient _client,
    BlobServiceClient _serviceClient
)
{
    private CounterItem CreateCounter(string operation)
        => _counterManager.Create($"Azure.BlobStorage.{_client.Name}.{operation}");

    public async Task<string?> GenerateSasLinkAsync(
        string filename,
        TimeSpan expiration,
        string resource = "b",
        BlobSasPermissions permissions = BlobSasPermissions.Read,
        CancellationToken cancellationToken = default
    )
    {
        using (CreateCounter(nameof(GenerateSasLinkAsync)))
        {
            var userDelegationKey = await _serviceClient.GetUserDelegationKeyAsync(
                startsOn: DateTimeOffset.UtcNow,
                expiresOn: DateTimeOffset.UtcNow.Add(expiration * 1.1),
                cancellationToken: cancellationToken
            );

            var blobClient = _client.GetBlobClient(filename);
            var builder = new BlobSasBuilder
            {
                BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                BlobName = blobClient.Name,
                Resource = resource,
                ExpiresOn = DateTimeOffset.Now.Add(expiration)
            };
            builder.SetPermissions(permissions);

            var sasUri = blobClient.GenerateUserDelegationSasUri(
                builder: builder,
                userDelegationKey: userDelegationKey
            );

            return sasUri.ToString();
        }
    }

    public async Task DeleteAsync(string filename)
    {
        using (CreateCounter("Delete"))
        {
            var blobClient = _client.GetBlobClient(filename);

            await blobClient.DeleteIfExistsAsync();
        }
    }

    public async Task UploadAsync(string filename, byte[] content)
    {
        using (CreateCounter("Upload"))
        {
            var binaryData = new BinaryData(content);
            await _client.UploadBlobAsync(filename, binaryData);
        }
    }

    public async Task UploadAsync(string filename, Stream contentStream)
    {
        using (CreateCounter("Upload"))
        {
            await _client.UploadBlobAsync(filename, contentStream);
        }
    }

    public async Task<bool> ExistsAsync(string filename)
    {
        using (CreateCounter("Exists"))
        {
            var blobClient = _client.GetBlobClient(filename);
            var exists = await blobClient.ExistsAsync();

            return exists?.Value ?? false;
        }
    }

    public async Task<byte[]?> DownloadAsync(string filename)
    {
        if (!await ExistsAsync(filename))
            return null;

        using (CreateCounter("Download"))
        {
            var blobClient = _client.GetBlobClient(filename);

            var response = await blobClient.DownloadContentAsync();
            return response?.Value is null ? null : response.Value.Content.ToArray();
        }
    }
}
