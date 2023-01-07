using GrillBot.App.Infrastructure.IO;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Extensions;
using GrillBot.Data.Resources.Misc;
using ImageMagick;

namespace GrillBot.App.Services.Images;

public sealed class WithoutAccidentRenderer : RendererBase, IDisposable
{
    private MagickImage Background { get; }
    private MagickImage Head { get; }
    private MagickImage Pliers { get; }

    private DataCacheManager DataCacheManager { get; }

    public WithoutAccidentRenderer(ProfilePictureManager profilePictureManager, DataCacheManager dataCacheManager)
        : base(null, profilePictureManager)
    {
        DataCacheManager = dataCacheManager;

        Background = new MagickImage(MiscResources.xDaysBackground);
        Head = new MagickImage(MiscResources.xDaysHead);
        Pliers = new MagickImage(MiscResources.xDaysPliers);
    }

    public override async Task<TemporaryFile> RenderAsync(IUser user, IGuild guild, IChannel channel, IMessage message, IDiscordInteraction interaction)
    {
        var daysCount = await GetLastErrorDays();

        var profilePicture = await ProfilePictureManager.GetOrCreatePictureAsync(user, size: 512);
        using var avatar = new MagickImageCollection(profilePicture.Data);

        using var image = RenderImage(avatar[0], daysCount);

        var tmpFile = new TemporaryFile("png");
        await image.WriteAsync(tmpFile.Path, MagickFormat.Png);

        return tmpFile;
    }

    private IMagickImage<byte> RenderImage(IMagickImage<byte> avatar, int daysCount)
    {
        var drawables = new Drawables();

        DrawNumber(drawables, daysCount);
        DrawAvatar(drawables, avatar);

        drawables
            .Composite(0, 0, CompositeOperator.Over, Head)
            .Composite(0, 0, CompositeOperator.Over, Pliers);

        var template = Background.Clone();
        drawables.Draw(template);
        return template;
    }

    private static void DrawNumber(Drawables drawables, int daysCount)
    {
        drawables
            .StrokeAntialias(true)
            .TextAntialias(true)
            .FillColor(MagickColors.Black)
            .TextAlignment(TextAlignment.Center)
            .Font("Open Sans")
            .FontPointSize(100)
            .Text(1090, 280, daysCount.ToString());
    }

    private static void DrawAvatar(Drawables drawables, IMagickImage<byte> avatar)
    {
        avatar.Resize(230, 230);
        avatar.RoundImage();
        avatar.Crop(230, 200);
        drawables.Composite(560, 270, CompositeOperator.Over, avatar);
    }

    private async Task<int> GetLastErrorDays()
    {
        var lastErorDate = await DataCacheManager.GetValueAsync("LastErrorDate");
        if (string.IsNullOrEmpty(lastErorDate)) return 0;

        var lastError = DateTime.Parse(lastErorDate);
        var totalDays = (DateTime.Now - lastError).TotalDays;
        return Convert.ToInt32(Math.Round(totalDays));
    }

    public void Dispose()
    {
        Background.Dispose();
        Head.Dispose();
        Pliers.Dispose();
    }
}
