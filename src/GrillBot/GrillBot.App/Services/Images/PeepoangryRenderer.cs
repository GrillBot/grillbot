using Discord;
using Discord.Commands;
using GrillBot.App.Extensions;
using GrillBot.App.Extensions.Discord;
using GrillBot.Data.Resources.Peepoangry;
using ImageMagick;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GrillBot.App.Services.Images
{
    public sealed class PeepoangryRenderer : RendererBase, IDisposable
    {
        private MagickImage AngryPeepo { get; }

        public PeepoangryRenderer(FileStorage.FileStorageFactory fileStorageFactory) : base(fileStorageFactory)
        {
            AngryPeepo = new MagickImage(PeepoangryResources.peepoangry);
        }

        public override async Task<string> RenderAsync(IUser user, ICommandContext commandContext)
        {
            var filename = user.CreateProfilePicFilename(64);
            var file = await Cache.GetFileInfoAsync("Peepoangry", filename);

            if (file.Exists)
                return file.FullName;

            (var profilePicture, var profilePictureInfo) = await GetProfilePictureAsync(user, 64);
            if (profilePictureInfo.Extension == ".gif" && !CanProcessGif(profilePictureInfo, commandContext.Guild))
            {
                filename = Path.ChangeExtension(filename, ".png");
                file = await Cache.GetFileInfoAsync("Peepoangry", filename);
                if (file.Exists)
                    return file.FullName;
            }

            try
            {
                if (file.Extension == ".gif")
                {
                    using var collection = new MagickImageCollection();

                    foreach (var userFrame in profilePicture)
                    {
                        userFrame.RoundImage();
                        var frame = RenderFrame(userFrame);

                        frame.AnimationDelay = userFrame.AnimationDelay;
                        frame.GifDisposeMethod = GifDisposeMethod.Background;
                        collection.Add(frame);
                    }

                    collection.Coalesce();
                    await collection.WriteAsync(file.FullName, MagickFormat.Gif);
                }
                else
                {
                    var avatarFrame = profilePicture[0];
                    avatarFrame.RoundImage();

                    using var frame = RenderFrame(avatarFrame);
                    await frame.WriteAsync(file.FullName, MagickFormat.Png);
                }
            }
            finally
            {
                profilePicture.Dispose();
            }

            return file.FullName;
        }

        private IMagickImage<byte> RenderFrame(IMagickImage<byte> avatarFrame)
        {
            var image = new MagickImage(MagickColors.Transparent, 250, 105);

            new Drawables()
                .Composite(20, 10, CompositeOperator.Over, avatarFrame)
                .Composite(115, -5, CompositeOperator.Over, AngryPeepo)
                .Draw(image);

            return image;
        }

        public void Dispose()
        {
            AngryPeepo?.Dispose();
        }
    }
}
