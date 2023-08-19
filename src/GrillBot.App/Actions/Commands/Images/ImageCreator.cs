using GrillBot.Cache.Services.Managers;
using GrillBot.Core.Services.ImageProcessing;
using GrillBot.Core.IO;

namespace GrillBot.App.Actions.Commands.Images;

public class ImageCreator : CommandAction
{
    private ProfilePictureManager ProfilePictureManager { get; }
    private IImageProcessingClient ImageProcessingClient { get; }

    public ImageCreator(ProfilePictureManager profilePictureManager, IImageProcessingClient imageProcessingClient)
    {
        ProfilePictureManager = profilePictureManager;
        ImageProcessingClient = imageProcessingClient;
    }

    public async Task<TemporaryFile> PeepoloveAsync(IUser? user)
    {
        var renderer = new PeepoloveRenderer(ProfilePictureManager, ImageProcessingClient);
        return await renderer.RenderAsync(user ?? Context.User, Context.Guild);
    }

    public async Task<TemporaryFile> PeepoangryAsync(IUser? user)
    {
        var renderer = new PeepoangryRenderer(ProfilePictureManager, ImageProcessingClient);
        return await renderer.RenderAsync(user ?? Context.User, Context.Guild);
    }
}
