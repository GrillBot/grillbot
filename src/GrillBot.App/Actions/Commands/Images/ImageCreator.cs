using GrillBot.Cache.Services.Managers;
using GrillBot.Core.Services.ImageProcessing;
using GrillBot.Core.IO;
using GrillBot.Core.Services.Common.Executor;

namespace GrillBot.App.Actions.Commands.Images;

public class ImageCreator : CommandAction
{
    private ProfilePictureManager ProfilePictureManager { get; }
    private readonly IServiceClientExecutor<IImageProcessingClient> _imageProcessingClient;

    public ImageCreator(ProfilePictureManager profilePictureManager, IServiceClientExecutor<IImageProcessingClient> imageProcessingClient)
    {
        ProfilePictureManager = profilePictureManager;
        _imageProcessingClient = imageProcessingClient;
    }

    public async Task<TemporaryFile> PeepoloveAsync(IUser? user)
    {
        var renderer = new PeepoloveRenderer(ProfilePictureManager, _imageProcessingClient);
        return await renderer.RenderAsync(user ?? Context.User, Context.Guild);
    }

    public async Task<TemporaryFile> PeepoangryAsync(IUser? user)
    {
        var renderer = new PeepoangryRenderer(ProfilePictureManager, _imageProcessingClient);
        return await renderer.RenderAsync(user ?? Context.User, Context.Guild);
    }
}
