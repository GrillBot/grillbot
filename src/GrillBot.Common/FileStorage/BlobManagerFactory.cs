using Azure.Identity;
using Azure.Storage.Blobs;
using GrillBot.Core.Managers.Performance;
using Microsoft.Extensions.Configuration;

namespace GrillBot.Common.FileStorage;

public class BlobManagerFactory(
    ICounterManager _counterManager,
    IConfiguration _configuration
)
{
    public async Task<BlobManager> CreateAsync(string containerName)
    {
        var authData = _configuration.GetRequiredSection("Auth:AzureIdentity");

        var accountAddress = new Uri(authData["StorageAccountUrl"]!);
        var credentials = new ClientSecretCredential(
            authData["TenantId"],
            authData["ClientId"],
            authData["ClientSecret"]
        );

        var client = new BlobServiceClient(accountAddress, credentials);
        var container = client.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync();

        return new BlobManager(_counterManager, container);
    }
}
