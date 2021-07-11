using Discord;
using Discord.Commands;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Services.FileStorage;
using ImageMagick;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GrillBot.App.Services.Images
{
    public abstract class RendererBase
    {
        protected FileStorageFactory FileStorageFactory { get; }
        protected FileStorage.FileStorage Cache { get; }

        protected RendererBase(FileStorageFactory fileStorageFactory)
        {
            FileStorageFactory = fileStorageFactory;
            Cache = FileStorageFactory.CreateCache();
        }

        protected async Task<(MagickImageCollection, FileInfo)> GetProfilePictureAsync(IUser user, int size)
        {
            var filename = user.CreateProfilePicFilename(size);
            var fileinfo = await Cache.GetProfilePictureInfoAsync(filename);

            if (!fileinfo.Exists)
            {
                var profilePicture = await user.DownloadAvatarAsync(size: (ushort)size);
                await Cache.StoreProfilePictureAsync(filename, profilePicture);
            }

            return (
                new MagickImageCollection(fileinfo),
                fileinfo
            );
        }

        public abstract Task<string> RenderAsync(IUser user, ICommandContext commandContext);

        protected bool CanProcessGif(FileInfo fileinfo, IGuild guild)
            => fileinfo.Length > 2 * (guild.CalculateFileUploadLimit() * 1024 * 1024 / 3);
    }
}
