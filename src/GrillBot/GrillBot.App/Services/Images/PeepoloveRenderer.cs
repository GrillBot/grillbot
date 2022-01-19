﻿using Discord.Commands;
using GrillBot.App.Extensions;
using GrillBot.App.Services.FileStorage;
using GrillBot.Data.Extensions.Discord;
using GrillBot.Data.Resources.Peepolove;
using ImageMagick;

namespace GrillBot.App.Services.Images
{
    public sealed class PeepoloveRenderer : RendererBase, IDisposable
    {
        private MagickImage Body { get; }
        private MagickImage Hands { get; }

        public PeepoloveRenderer(FileStorageFactory fileStorageFactory) : base(fileStorageFactory)
        {
            Body = new MagickImage(PeepoloveResources.Body);
            Hands = new MagickImage(PeepoloveResources.Hands);
        }

        public override async Task<string> RenderAsync(IUser user, ICommandContext commandContext)
        {
            var filename = user.CreateProfilePicFilename(256);
            var file = await Cache.GetFileInfoAsync("Peepolove", filename);

            if (file.Exists)
                return file.FullName;

            (var profilePicture, var profilePictureInfo) = await GetProfilePictureAsync(user, 256);
            if (profilePictureInfo.Extension == ".gif" && !CanProcessGif(profilePictureInfo, commandContext.Guild))
            {
                filename = Path.ChangeExtension(filename, ".png");
                file = await Cache.GetFileInfoAsync("Peepolove", filename);
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
                        userFrame.Resize(180, 180);
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
                    avatarFrame.Resize(180, 180);
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
            var body = Body.Clone();

            new Drawables()
                .Composite(5, 312, CompositeOperator.Over, avatarFrame)
                .Composite(0, 0, CompositeOperator.Over, Hands)
                .Draw(body);

            body.Crop(new MagickGeometry(0, 115, 512, 397));
            return body;
        }

        public void Dispose()
        {
            Body?.Dispose();
            Hands?.Dispose();
        }
    }
}
