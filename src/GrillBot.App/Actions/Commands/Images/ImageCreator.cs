using GrillBot.Cache.Services.Managers;
using GrillBot.Common.FileStorage;

namespace GrillBot.App.Actions.Commands.Images;

public class ImageCreator : CommandAction
{
    private FileStorageFactory FileStorageFactory { get; }
    private ProfilePictureManager ProfilePictureManager { get; }

    public ImageCreator(FileStorageFactory fileStorageFactory, ProfilePictureManager profilePictureManager)
    {
        FileStorageFactory = fileStorageFactory;
        ProfilePictureManager = profilePictureManager;
    }

    public async Task<string> PeepoloveAsync(IUser? user)
    {
        using var renderer = new PeepoloveRenderer(FileStorageFactory, ProfilePictureManager);
        return await renderer.RenderAsync(user ?? Context.User, Context.Guild);
    }

    public async Task<string> PeepoangryAsync(IUser? user)
    {
        using var renderer = new PeepoangryRenderer(FileStorageFactory, ProfilePictureManager);
        return await renderer.RenderAsync(user ?? Context.User, Context.Guild);
    }
}
