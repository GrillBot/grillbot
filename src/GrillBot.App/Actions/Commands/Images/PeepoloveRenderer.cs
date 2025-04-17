using GrillBot.Cache.Services.Managers;
using GrillBot.Core.Services.ImageProcessing;
using GrillBot.Core.Services.ImageProcessing.Models;
using GrillBot.Core.IO;
using GrillBot.Core.Services.Common.Executor;

namespace GrillBot.App.Actions.Commands.Images;

public sealed class PeepoloveRenderer
{
    private ProfilePictureManager ProfilePictureManager { get; }
    private readonly IServiceClientExecutor<IImageProcessingClient> _imageProcessingClient;

    public PeepoloveRenderer(ProfilePictureManager profilePictureManager, IServiceClientExecutor<IImageProcessingClient> imageProcessingClient)
    {
        ProfilePictureManager = profilePictureManager;
        _imageProcessingClient = imageProcessingClient;
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

        var image = await _imageProcessingClient.ExecuteRequestAsync((c, ctx) => c.CreatePeepoloveImageAsync(request, ctx.CancellationToken));
        var result = new TemporaryFile(request.AvatarInfo.Type);

        await result.WriteStreamAsync(image);
        return result;
    }
}
