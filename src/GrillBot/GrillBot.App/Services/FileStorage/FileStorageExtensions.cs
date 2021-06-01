using System.IO;
using System.Threading.Tasks;

namespace GrillBot.App.Services.FileStorage
{
    static public class FileStorageExtensions
    {
        #region FileStorageFactory

        static public FileStorage CreateCache(this FileStorageFactory factory) => factory.Create("Cache");

        #endregion

        #region FileStorage

        static public Task<byte[]> GetProfilePictureAsync(this FileStorage storage, string filename) => storage.GetFileAsync("ProfilePictures", filename);
        static public Task StoreProfilePictureAsync(this FileStorage storage, string filename, byte[] content) => storage.StoreFileAsync("ProfilePictures", filename, content);
        static public Task<FileInfo> GetProfilePictureInfoAsync(this FileStorage storage, string filename) => storage.GetFileInfoAsync("ProfilePictures", filename);

        #endregion
    }
}
