using ImageMagick;

namespace GrillBot.App.Extensions
{
    static public class ImageExtensions
    {
        static public void RoundImage(this IMagickImage<byte> image)
        {
            image.Format = MagickFormat.Png;
            image.Alpha(AlphaOption.On);

            using var copy = image.Clone();

            copy.Distort(DistortMethod.DePolar, 0);
            copy.VirtualPixelMethod = VirtualPixelMethod.HorizontalTile;
            copy.BackgroundColor = MagickColors.None;
            copy.Distort(DistortMethod.Polar, 0);

            image.Compose = CompositeOperator.DstIn;
            image.Composite(copy, CompositeOperator.CopyAlpha);
        }

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

        static public string CutToImageWidth(this string str, int width, string font, double fontSize)
        {
            var drawables = new Drawables()
                .Font(font)
                .FontPointSize(fontSize);

            var builder = new StringBuilder();
            foreach (var character in str)
            {
                var metrics = drawables.FontTypeMetrics(builder.ToString() + character);
                if (metrics.TextWidth >= width) break;
                builder.Append(character);
            }

            return builder.ToString();
        }
    }
}
