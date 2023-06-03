using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Services.ImageProcessing;
using GrillBot.Common.Services.ImageProcessing.Models;
using GrillBot.Core.IO;

namespace GrillBot.App.Actions.Commands.Images;

public sealed class PeepoloveRenderer
{
    private ProfilePictureManager ProfilePictureManager { get; }
    private IImageProcessingClient ImageProcessingClient { get; }

    public PeepoloveRenderer(ProfilePictureManager profilePictureManager, IImageProcessingClient imageProcessingClient)
    {
        ProfilePictureManager = profilePictureManager;
        ImageProcessingClient = imageProcessingClient;
    }

    public async Task<TemporaryFile> RenderAsync(IUser user, IGuild guild)
    {
        var profilePicture = await ProfilePictureManager.GetOrCreatePictureAsync(user, 256);

        var request = new PeepoRequest
        {
            AvatarInfo = new AvatarInfo
            {
                Type = profilePicture.IsAnimated ? "gif" : "png",
                AvatarContent = profilePicture.Data,
                AvatarId = profilePicture.AvatarId
            },
            UserId = user.Id.ToString(),
            GuildUploadLimit = (long)guild.MaxUploadLimit
        };

        var image = await ImageProcessingClient.CreatePeepoloveImageAsync(request);
        var result = new TemporaryFile(request.AvatarInfo.Type);

        await File.WriteAllBytesAsync(result.Path, image);
        return result;
    }
}
