using Microsoft.Extensions.Configuration;

namespace GrillBot.Common.FileStorage;

public class FileStorage
{
    private DirectoryInfo StorageInfo { get; }

    public FileStorage(IConfiguration configuration)
    {
        StorageInfo = new DirectoryInfo(configuration.GetValue<string>("Location")!);

        if (!StorageInfo.Exists)
            StorageInfo.Create();
    }

    public Task<FileInfo> GetFileInfoAsync(string filename)
    {
        var path = Path.Combine(StorageInfo.FullName, filename);
        return Task.FromResult(new FileInfo(path));
    }
}
