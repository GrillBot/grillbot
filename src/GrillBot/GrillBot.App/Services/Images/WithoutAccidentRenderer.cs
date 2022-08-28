using GrillBot.App.Infrastructure.IO;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.FileStorage;
using GrillBot.Data.Resources.Misc;
using ImageMagick;

namespace GrillBot.App.Services.Images;

public sealed class WithoutAccidentRenderer : RendererBase, IDisposable
{
    private MagickImage Template { get; }

    public WithoutAccidentRenderer(FileStorageFactory fileStorageFactory, ProfilePictureManager profilePictureManager)
        : base(fileStorageFactory, profilePictureManager)
    {
        Template = new MagickImage(MiscResources.xDays);
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
        var drawables = new Drawables()
            .StrokeAntialias(true)
            .TextAntialias(true)
            .FillColor(MagickColors.Black)
            .TextAlignment(TextAlignment.Center)
            .Font("Open Sans")
            .FontPointSize(80)
            .Text(1085, 260, daysCount.ToString());

        avatar.Resize(300, 300);
        avatar.RoundImage();
        drawables.Composite(540, 170, CompositeOperator.Over, avatar);

        var template = Template.Clone();
        drawables.Draw(template);
        return template;
    }

    private async Task<int> GetLastErrorDays()
    {
        var lastErrorInfo = await Cache.GetFileInfoAsync("Common", "LastErrorDate.txt");
        if (!lastErrorInfo.Exists)
            return 0;

        var lastErrorData = await File.ReadAllTextAsync(lastErrorInfo.FullName);
        var lastError = DateTime.Parse(lastErrorData.Trim());

        var totalDays = (DateTime.Now - lastError).TotalDays;
        return Convert.ToInt32(Math.Round(totalDays));
    }

    public void Dispose()
    {
        Template.Dispose();
    }
}
