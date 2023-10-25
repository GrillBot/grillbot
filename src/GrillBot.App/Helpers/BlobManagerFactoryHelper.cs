using GrillBot.Common.FileStorage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace GrillBot.App.Helpers;

public class BlobManagerFactoryHelper
{
    private BlobManagerFactory Factory { get; }
    private IWebHostEnvironment Environment { get; }

    public BlobManagerFactoryHelper(BlobManagerFactory blobManagerFactory, IWebHostEnvironment environment)
    {
        Factory = blobManagerFactory;
        Environment = environment;
    }

    private string CreateShortcut()
        => Environment.IsDevelopment() ? "dev" : "prod";

    public async Task<BlobManager> CreateAsync(string containerName)
        => await Factory.CreateAsync($"{containerName}-{CreateShortcut()}");
}
