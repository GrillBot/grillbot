using Azure.Storage.Blobs;
using GrillBot.Core.Managers.Performance;
using Microsoft.Extensions.Configuration;

namespace GrillBot.Common.FileStorage;

public class BlobManagerFactory
{
    private ICounterManager CounterManager { get; }
    private IConfiguration Configuration { get; }

    public BlobManagerFactory(ICounterManager counterManager, IConfiguration configuration)
    {
        CounterManager = counterManager;
        Configuration = configuration;
    }

    public async Task<BlobManager> CreateAsync(string containerName)
    {
        var connectionString = Configuration.GetConnectionString("StorageAccount");

        var containerClient = new BlobContainerClient(connectionString, containerName);
        await containerClient.CreateIfNotExistsAsync();

        return new BlobManager(CounterManager, containerClient);
    }
}
