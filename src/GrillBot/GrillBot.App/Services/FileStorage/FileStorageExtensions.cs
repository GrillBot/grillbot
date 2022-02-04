namespace GrillBot.App.Services.FileStorage
{
    static public class FileStorageExtensions
    {
        #region FileStorageFactory

        static public IFileStorage CreateCache(this FileStorageFactory factory) => factory.Create("Cache");

        #endregion

        #region FileStorage

        static public Task<byte[]> GetProfilePictureAsync(this IFileStorage storage, string filename) => storage.GetFileAsync("ProfilePictures", filename);
        static public Task StoreProfilePictureAsync(this IFileStorage storage, string filename, byte[] content) => storage.StoreFileAsync("ProfilePictures", filename, content);
        static public Task<FileInfo> GetProfilePictureInfoAsync(this IFileStorage storage, string filename) => storage.GetFileInfoAsync("ProfilePictures", filename);

        #endregion
    }
}
