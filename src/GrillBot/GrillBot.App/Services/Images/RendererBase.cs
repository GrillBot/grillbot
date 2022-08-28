using Discord.Commands;
using GrillBot.App.Infrastructure.IO;
using GrillBot.Cache.Entity;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Extensions.Discord;
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

    public virtual Task<string> RenderAsync(IUser user, ICommandContext commandContext)
    {
        throw new NotImplementedException();
    }

    public virtual Task<TemporaryFile> RenderAsync(IUser user, IGuild guild, IChannel channel, IMessage message, IDiscordInteraction interaction)
    {
        throw new NotImplementedException();
    }

    protected static bool CanProcessGif(ProfilePicture profilePicture, IGuild guild)
        => profilePicture.Data.Length <= 2 * (guild.CalculateFileUploadLimit() * 1024 * 1024 / 3);
}
