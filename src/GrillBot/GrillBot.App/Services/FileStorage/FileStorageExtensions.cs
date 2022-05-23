namespace GrillBot.App.Services.FileStorage
{
    static public class FileStorageExtensions
    {
        #region FileStorageFactory

        static public IFileStorage CreateCache(this FileStorageFactory factory) => factory.Create("Cache");

        #endregion
    }
}
