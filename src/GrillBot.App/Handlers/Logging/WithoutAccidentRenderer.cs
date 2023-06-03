using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Services.ImageProcessing;
using GrillBot.Common.Services.ImageProcessing.Models;
using GrillBot.Core.IO;

namespace GrillBot.App.Handlers.Logging;

public class WithoutAccidentRenderer
{
    private DataCacheManager DataCacheManager { get; }
    private ProfilePictureManager ProfilePictureManager { get; }
    private IImageProcessingClient ImageProcessingClient { get; }

    public WithoutAccidentRenderer(ProfilePictureManager profilePictureManager, DataCacheManager dataCacheManager, IImageProcessingClient imageProcessingClient)
    {
        DataCacheManager = dataCacheManager;
        ProfilePictureManager = profilePictureManager;
        ImageProcessingClient = imageProcessingClient;
    }

    public async Task<TemporaryFile> RenderAsync(IUser user)
    {
        var profilePicture = await ProfilePictureManager.GetOrCreatePictureAsync(user, size: 512);
        var days = await GetLastErrorDays();

        var request = new WithoutAccidentImageRequest
        {
            AvatarInfo = new AvatarInfo
            {
                Type = "png",
                AvatarContent = profilePicture.Data,
                AvatarId = profilePicture.AvatarId
            },
            DaysCount = days,
            UserId = user.Id.ToString()
        };

        var image = await ImageProcessingClient.CreateWithoutAccidentImageAsync(request);
        var tmpFile = new TemporaryFile("png");
        await File.WriteAllBytesAsync(tmpFile.Path, image);

        return tmpFile;
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
