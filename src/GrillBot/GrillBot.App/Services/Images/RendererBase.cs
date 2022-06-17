using Discord.Commands;
using GrillBot.Cache.Entity;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.FileStorage;

namespace GrillBot.App.Services.Images;

public abstract class RendererBase
{
    protected IFileStorage Cache { get; }
    protected ProfilePictureManager ProfilePictureManager { get; }

    protected RendererBase(FileStorageFactory fileStorageFactory, ProfilePictureManager profilePictureManager)
    {
        Cache = fileStorageFactory.Create("Cache");
        ProfilePictureManager = profilePictureManager;
    }

    public abstract Task<string> RenderAsync(IUser user, ICommandContext commandContext);

    protected static bool CanProcessGif(ProfilePicture profilePicture, IGuild guild)
        => profilePicture.Data.Length <= 2 * (guild.CalculateFileUploadLimit() * 1024 * 1024 / 3);
}
