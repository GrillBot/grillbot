namespace GrillBot.App.Services.FileStorage
{
    public class FileStorageFactory
    {
        private IConfiguration Configuration { get; }

        public FileStorageFactory(IConfiguration configuration)
        {
            Configuration = configuration.GetSection("FileStorage");
        }

        public FileStorage Create(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
                throw new ArgumentNullException(nameof(categoryName));

            var configuration = Configuration.GetSection(categoryName);

            if (!configuration.Exists())
                throw new ArgumentNullException(nameof(categoryName), $"Configuration for storage {categoryName} not found.");

            return new FileStorage(configuration);
        }
    }
}
