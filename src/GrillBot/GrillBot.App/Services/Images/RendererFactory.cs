using GrillBot.Cache.Services.Managers;
using GrillBot.Common.FileStorage;

namespace GrillBot.App.Services.Images;

public class RendererFactory
{
    protected FileStorageFactory FileStorage { get; }
    protected ProfilePictureManager ProfilePictureManager { get; }

    public RendererFactory(FileStorageFactory fileStorage, ProfilePictureManager profilePictureManager)
    {
        FileStorage = fileStorage;
        ProfilePictureManager = profilePictureManager;
    }

    public virtual RendererBase Create<TRenderer>() where TRenderer : RendererBase
    {
        var rendererType = typeof(TRenderer);

        if (rendererType == typeof(WithoutAccidentRenderer))
            return new WithoutAccidentRenderer(FileStorage, ProfilePictureManager) as TRenderer;
        if (rendererType == typeof(PeepoangryRenderer))
            return new PeepoangryRenderer(FileStorage, ProfilePictureManager) as TRenderer;
        if (rendererType == typeof(PeepoloveRenderer))
            return new PeepoloveRenderer(FileStorage, ProfilePictureManager) as TRenderer;

        throw new NotSupportedException();
    }
}
