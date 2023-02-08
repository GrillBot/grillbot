using GrillBot.App.Infrastructure.IO;
using GrillBot.Cache.Entity;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Extensions;
using GrillBot.Common.Services.Graphics;
using GrillBot.Common.Services.Graphics.Models.Images;
using ImageMagick;

namespace GrillBot.App.Handlers.Logging;

public class WithoutAccidentRenderer
{
    private DataCacheManager DataCacheManager { get; }
    private ProfilePictureManager ProfilePictureManager { get; }
    private IGraphicsClient GraphicsClient { get; }

    public WithoutAccidentRenderer(ProfilePictureManager profilePictureManager, DataCacheManager dataCacheManager, IGraphicsClient graphicsClient)
    {
        DataCacheManager = dataCacheManager;
        ProfilePictureManager = profilePictureManager;
        GraphicsClient = graphicsClient;
    }

    public async Task<TemporaryFile> RenderAsync(IUser user)
    {
        var profilePicture = await ProfilePictureManager.GetOrCreatePictureAsync(user, size: 512);
        var request = new WithoutAccidentRequestData
        {
            Days = await GetLastErrorDays(),
            ProfilePicture = ReadAvatarToBase64(profilePicture)
        };

        var image = await GraphicsClient.CreateWithoutAccidentImage(request);
        var tmpFile = new TemporaryFile("png");
        await File.WriteAllBytesAsync(tmpFile.Path, image);

        return tmpFile;
    }

    private static string ReadAvatarToBase64(ProfilePicture profilePicture)
    {
        using var avatarCollection = new MagickImageCollection(profilePicture.Data);
        return avatarCollection[0].ToBase64();
    }

    private async Task<int> GetLastErrorDays()
    {
        var lastErorDate = await DataCacheManager.GetValueAsync("LastErrorDate");
        if (string.IsNullOrEmpty(lastErorDate)) return 0;

        var lastError = DateTime.Parse(lastErorDate);
        var totalDays = (DateTime.Now - lastError).TotalDays;
        return Convert.ToInt32(Math.Round(totalDays));
    }
}
