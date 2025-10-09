using GrillBot.Cache.Services.Managers;
using ImageProcessing;
using ImageProcessing.Models;
using GrillBot.Core.IO;
using GrillBot.Core.Services.Common.Executor;
using System.Diagnostics;

namespace GrillBot.App.Handlers.Logging;

public class WithoutAccidentRenderer(
    ProfilePictureManager _profilePictureManager,
    DataCacheManager _dataCacheManager,
    IServiceClientExecutor<IImageProcessingClient> _imageProcessingClient
)
{
    public async Task<TemporaryFile> RenderAsync(IUser user)
    {
        var profilePicture = await _profilePictureManager.GetOrCreatePictureAsync(user, size: 512);
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

        var image = await _imageProcessingClient.ExecuteRequestAsync((c, ctx) => c.CreateWithoutAccidentImageAsync(request, ctx.CancellationToken));
        var tmpFile = new TemporaryFile("png");

        await tmpFile.WriteStreamAsync(image);
        return tmpFile;
    }

    private async Task<int> GetLastErrorDays()
    {
        var lastErrorDate = await _dataCacheManager.GetValueAsync<DateTime>("GrillBot_LastErrorDate");
        if (lastErrorDate == DateTime.MinValue)
            lastErrorDate = Process.GetCurrentProcess().StartTime;

        var now = DateTime.Now;
        var totalDays = (now - lastErrorDate).TotalDays;
        return Convert.ToInt32(Math.Round(totalDays));
    }
}
