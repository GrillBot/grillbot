using GrillBot.Cache.Services.Managers;
using GrillBot.Core.Services.ImageProcessing;
using GrillBot.Core.Services.ImageProcessing.Models;
using GrillBot.Core.IO;

namespace GrillBot.App.Handlers.Logging;

public class WithoutAccidentRenderer
{
    private readonly DataCacheManager _dataCacheManager;
    private ProfilePictureManager ProfilePictureManager { get; }
    private IImageProcessingClient ImageProcessingClient { get; }

    public WithoutAccidentRenderer(ProfilePictureManager profilePictureManager, DataCacheManager dataCacheManager, IImageProcessingClient imageProcessingClient)
    {
        _dataCacheManager = dataCacheManager;
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

        await using var ms = new MemoryStream();
        await image.CopyToAsync(ms);

        await tmpFile.WriteAllBytesAsync(ms.ToArray());

        return tmpFile;
    }

    private async Task<int> GetLastErrorDays()
    {
        var lastErrorDate = await _dataCacheManager.GetValueAsync<DateTime>("LastErrorDate");
        if (lastErrorDate == DateTime.MinValue) return 0;

        var now = DateTime.Now;
        var totalDays = (now - lastErrorDate).TotalDays;
        return Convert.ToInt32(Math.Round(totalDays));
    }
}
