using GrillBot.App.Infrastructure.IO;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Services.Graphics;
using ImageMagick;

namespace GrillBot.App.Actions.Commands.Images;

public sealed class PeepoangryRenderer
{
    private ProfilePictureManager ProfilePictureManager { get; }
    private IGraphicsClient GraphicsClient { get; }

    public PeepoangryRenderer(ProfilePictureManager profilePictureManager, IGraphicsClient graphicsClient)
    {
        ProfilePictureManager = profilePictureManager;
        GraphicsClient = graphicsClient;
    }

    public async Task<TemporaryFile> RenderAsync(IUser user, IGuild guild)
    {
        var profilePicture = await ProfilePictureManager.GetOrCreatePictureAsync(user, 64);
        var profilePictureFrames = new List<byte[]>();
        var result = new TemporaryFile(user.HaveAnimatedAvatar() ? ".gif" : ".png");

        if (profilePicture.IsAnimated && !MessageHelper.CanSendAttachment(profilePicture.Data.Length, guild))
            result.ChangeExtension(".png");

        using var originalImage = new MagickImageCollection(profilePicture.Data);

        if (Path.GetExtension(result.Path) == ".gif")
        {
            originalImage.Coalesce();
            profilePictureFrames.AddRange(originalImage.Select(userFrame => userFrame.ToByteArray()));
        }
        else
        {
            profilePictureFrames.Add(originalImage[0].ToByteArray());
        }

        var createdFrames = await GraphicsClient.CreatePeepoAngryAsync(profilePictureFrames);
        if (createdFrames.Count == 1)
        {
            // User not have profile picture.
            using var img = new MagickImage(createdFrames[0]);
            await img.WriteAsync(result.Path, MagickFormat.Png);
        }
        else
        {
            // User have gif
            var framesQuery = createdFrames.Select(o =>
            {
                var frame = new MagickImage(o, MagickFormat.Png);
                frame.GifDisposeMethod = GifDisposeMethod.None;

                return frame;
            });

            using var collection = new MagickImageCollection(framesQuery);
            await collection.WriteAsync(result.Path, MagickFormat.Gif);
        }

        return result;
    }
}
