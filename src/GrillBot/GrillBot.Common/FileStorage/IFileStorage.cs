namespace GrillBot.Common.FileStorage;

public interface IFileStorage
{
    Task StoreFileAsync(string subcategory, string filename, byte[] content);
    Task<FileInfo> GetFileInfoAsync(string subcategory, string filename);
}
