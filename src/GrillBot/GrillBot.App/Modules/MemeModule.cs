using Discord;
using Discord.Commands;
using GrapeCity.Documents.Imaging;
using GrillBot.App.Extensions;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Services.FileStorage;
using GrillBot.Data.Resources.Peepolove;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using SysDraw = System.Drawing;

namespace GrillBot.App.Modules
{
    [Name("Náhodné věci")]
    public class MemeModule : Infrastructure.ModuleBase
    {
        private FileStorageFactory FileStorageFactory { get; }

        public MemeModule(FileStorageFactory fileStorage)
        {
            FileStorageFactory = fileStorage;
        }

        #region Peepolove

        [Command("peepolove")]
        [Alias("love")]
        public async Task PeepoloveAsync([Name("id/tag/jmeno_uzivatele")] IUser user = null)
        {
            if (user == null) user = Context.User;
            var cache = FileStorageFactory.CreateCache();

            var filename = user.CreateProfilePicFilename(256);
            var file = await cache.GetFileInfoAsync("Peepolove", filename);
            bool withException = false;

            try
            {
                if (file.Exists) return;

                var profileImageInfo = await cache.GetProfilePictureInfoAsync(filename);
                if (!profileImageInfo.Exists)
                    await cache.StoreProfilePictureAsync(filename, await user.DownloadAvatarAsync(size: 256));

                if (file.Extension == ".gif" && profileImageInfo.Length > 2 * ((Context.Guild.CalculateFileUploadLimit() * 1024 * 1024) / 3))
                {
                    filename = Path.ChangeExtension(filename, ".png");
                    file = await cache.GetFileInfoAsync("Peepolove", filename);
                    if (file.Exists) return;
                }

                using var profilePictureImage = SysDraw.Image.FromFile(profileImageInfo.FullName);

                if (file.Extension == ".gif")
                {
                    using var gcBitmap = new GcBitmap();
                    using var gifWriter = new GcGifWriter(file.FullName);
                    var delay = profilePictureImage.CalculateGifDelay();

                    foreach (var profilePictureFrame in profilePictureImage.SplitGifIntoFrames())
                    {
                        try
                        {
                            using var rounded = profilePictureFrame.RoundImage();
                            using var frame = RenderPeepoloveFrame(rounded);

                            using var ms = new MemoryStream();
                            frame.Save(ms, SysDraw.Imaging.ImageFormat.Png);

                            gcBitmap.Load(ms.ToArray());
                            gifWriter.AppendFrame(gcBitmap, disposalMethod: GifDisposalMethod.RestoreToBackgroundColor, delayTime: delay);
                        }
                        finally
                        {
                            profilePictureFrame.Dispose();
                        }
                    }
                }
                else if (file.Extension == ".png")
                {
                    using var rounded = profilePictureImage.RoundImage();
                    using var resized = rounded.ResizeImage(256, 256);

                    using var frame = RenderPeepoloveFrame(resized);
                    frame.Save(file.FullName, SysDraw.Imaging.ImageFormat.Png);
                }
            }
            catch (Exception) { withException = true; }
            finally
            {
                if (File.Exists(file.FullName) && !withException)
                    await ReplyFileAsync(file.FullName, false);
            }
        }

        private static SysDraw.Image RenderPeepoloveFrame(SysDraw.Image profilePicture)
        {
            using var ms = new MemoryStream(PeepoloveResources.Body);
            using var body = new Bitmap(ms);
            using var graphics = Graphics.FromImage(body);

            graphics.DrawImage(profilePicture, new Rectangle(5, 312, 180, 180));

            using var handsStream = new MemoryStream(PeepoloveResources.Hands);
            using var hands = new Bitmap(handsStream);
            graphics.DrawImage(hands, new Rectangle(0, 0, 512, 512));

            return (body as SysDraw.Image).CropImage(new Rectangle(0, 115, 512, 397));
        }

        #endregion
    }
}
