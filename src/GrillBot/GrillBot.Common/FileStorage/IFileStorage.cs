namespace GrillBot.Common.FileStorage;

public interface IFileStorage
{
    Task<byte[]?> GetFileAsync(string subcategory, string filename);
    Task StoreFileAsync(string subcategory, string filename, byte[] content);
    Task<FileInfo> GetFileInfoAsync(string subcategory, string filename);
}
