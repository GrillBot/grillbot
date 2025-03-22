using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using GrillBot.Core.Managers.Performance;

namespace GrillBot.Common.FileStorage;

public class BlobManager
{
    private ICounterManager CounterManager { get; }
    private BlobContainerClient Client { get; }

    public BlobManager(ICounterManager counterManager, BlobContainerClient client)
    {
        CounterManager = counterManager;
        Client = client;
    }

    private CounterItem CreateCounter(string operation)
        => CounterManager.Create($"Azure.BlobStorage.{Client.Name}.{operation}");

    public string? GenerateSasLink(string filename, TimeSpan expiration, string resource = "b", BlobSasPermissions permissions = BlobSasPermissions.Read)
    {
        using (CreateCounter(nameof(GenerateSasLink)))
        {
            var blobClient = Client.GetBlobClient(filename);
            if (!blobClient.CanGenerateSasUri)
                return null;

            var builder = new BlobSasBuilder
            {
                BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                BlobName = blobClient.Name,
                Resource = resource,
                ExpiresOn = DateTimeOffset.Now.Add(expiration)
            };
            builder.SetPermissions(permissions);

            return blobClient.GenerateSasUri(builder).ToString();
        }
    }

    public async Task DeleteAsync(string filename)
    {
        using (CreateCounter("Delete"))
        {
            var blobClient = Client.GetBlobClient(filename);

            await blobClient.DeleteIfExistsAsync();
        }
    }

    public async Task UploadAsync(string filename, byte[] content)
    {
        using (CreateCounter("Upload"))
        {
            var binaryData = new BinaryData(content);
            await Client.UploadBlobAsync(filename, binaryData);
        }
    }

    public async Task UploadAsync(string filename, Stream contentStream)
    {
        using (CreateCounter("Upload"))
        {
            await Client.UploadBlobAsync(filename, contentStream);
        }
    }

    public async Task<bool> ExistsAsync(string filename)
    {
        using (CreateCounter("Exists"))
        {
            var blobClient = Client.GetBlobClient(filename);
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
            var blobClient = Client.GetBlobClient(filename);

            var response = await blobClient.DownloadContentAsync();
            return response?.Value is null ? null : response.Value.Content.ToArray();
        }
    }
}
