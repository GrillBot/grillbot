using ImageMagick;

namespace GrillBot.Common.Extensions;

public static class ImageExtensions
{
    public static MagickColor GetDominantColor(this MagickImage image)
    {
        using var clone = image.Clone();
        clone.HasAlpha = false;

        var histogram = clone.Histogram();
        return new MagickColor(histogram.Aggregate((x, y) => x.Value > y.Value ? x : y).Key);
    }

    public static MagickColor CreateDarkerBackgroundColor(this MagickColor color)
    {
        var tmpColor = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        tmpColor = tmpColor.GetBrightness() <= 0.2 ? 
            System.Drawing.Color.FromArgb(25, System.Drawing.Color.White) : 
            System.Drawing.Color.FromArgb(100, System.Drawing.Color.Black);

        return MagickColor.FromRgba(tmpColor.R, tmpColor.G, tmpColor.B, tmpColor.A);
    }
}
