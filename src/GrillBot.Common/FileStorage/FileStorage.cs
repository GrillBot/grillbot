using Microsoft.Extensions.Configuration;

namespace GrillBot.Common.FileStorage;

public class FileStorage : IFileStorage
{
    private DirectoryInfo StorageInfo { get; }

    public FileStorage(IConfiguration configuration)
    {
        StorageInfo = new DirectoryInfo(configuration.GetValue<string>("Location"));

        if (!StorageInfo.Exists)
            StorageInfo.Create();
    }

    public Task StoreFileAsync(string subcategory, string filename, byte[] content)
    {
        var path = BuildPath(subcategory, filename);
        return File.WriteAllBytesAsync(path, content);
    }

    public Task<FileInfo> GetFileInfoAsync(string subcategory, string filename)
    {
        var path = BuildPath(subcategory, filename);
        return Task.FromResult(new FileInfo(path));
    }

    private string BuildPath(string category, string filename)
    {
        var categoryPath = Path.Combine(StorageInfo.FullName, category);

        if (!Directory.Exists(categoryPath))
            Directory.CreateDirectory(categoryPath);

        return Path.Combine(categoryPath, filename);
    }
}
