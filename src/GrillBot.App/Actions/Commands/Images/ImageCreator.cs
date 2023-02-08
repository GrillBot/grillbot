using GrillBot.App.Infrastructure.IO;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Services.Graphics;

namespace GrillBot.App.Actions.Commands.Images;

public class ImageCreator : CommandAction
{
    private ProfilePictureManager ProfilePictureManager { get; }
    private IGraphicsClient GraphicsClient { get; }

    public ImageCreator(ProfilePictureManager profilePictureManager, IGraphicsClient graphicsClient)
    {
        ProfilePictureManager = profilePictureManager;
        GraphicsClient = graphicsClient;
    }

    public async Task<TemporaryFile> PeepoloveAsync(IUser? user)
    {
        var renderer = new PeepoloveRenderer(ProfilePictureManager, GraphicsClient);
        return await renderer.RenderAsync(user ?? Context.User, Context.Guild);
    }

    public async Task<TemporaryFile> PeepoangryAsync(IUser? user)
    {
        var renderer = new PeepoangryRenderer(ProfilePictureManager, GraphicsClient);
        return await renderer.RenderAsync(user ?? Context.User, Context.Guild);
    }
}
